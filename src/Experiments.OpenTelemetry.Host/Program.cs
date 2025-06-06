using System.Diagnostics;
using Autofac;
using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Library1;
using Experiments.OpenTelemetry.Library2;
using Experiments.OpenTelemetry.Telemetry;
using Functional;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Host;

internal sealed class Program
{
    #region Fields

    private static bool _disposed;
    private static ActivityScheduler? _entrypointScheduler;
    private static ActivityScheduler? _activityScheduler;
    private static CancellationTokenSource? _cts;
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _logger;
    private static IContainer? _scope;
    private static readonly object _mutex = new();
    private static readonly List<IDisposable> _disposables = [];

    #endregion

    static async Task Main()
    {
        try
        {
            _cts = new CancellationTokenSource();

            Init(_cts.Token);
            await Run(_cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Host execution error");
        }
        finally
        {
            ShutDown();
        }
    }

    private static void Init(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Console.CancelKeyPress += Exit;

        _scope = BuildContainer();

        var logger = _scope.Resolve<ILogger>();
        var telemetryCollector = _scope.Resolve<ITelemetryCollector>();
        var configuration = _scope.Resolve<IHostConfiguration>();

        void onEnqueueActivity(string activityUid, int activityQueueLength, Option<WorkItemsBatchDescriptor> workItemsBatchDescriptor)
            => workItemsBatchDescriptor.Match(
                    () => telemetryCollector.UpdateActivityQueueLength(activityUid, activityQueueLength),
                    descriptor => telemetryCollector.UpdateActivityQueueLength(activityUid, activityQueueLength, descriptor.SourceType)
                );

        _entrypointScheduler = new ActivityScheduler(
            logger,
            configuration.ActivityQueueLimit,
            onEnqueueActivity,
            cancellationToken
        );

        _activityScheduler = new ActivityScheduler(
            logger,
            configuration.ActivityQueueLimit,
            onEnqueueActivity,
            cancellationToken
        );

        var resolveActivity = new Func<IContainer, ActivityDescriptor, IProcessFlowJobActivity>(
            ResolveActivity).Curry()(_scope);

        var entryPointActivityExecutor = new ActivityExecutor(() => configuration.MaxConcurrentExecutingActivities,
            resolveActivity, logger, cancellationToken);

        var activityExecutor = new ActivityExecutor(() => configuration.MaxConcurrentExecutingActivities,
            resolveActivity, logger, cancellationToken);

        var commandServer = _scope.Resolve<CommandServer>(new NamedParameter("port", 5000));

        Task.Run(
            async () => await commandServer.RunAsync(cancellationToken).ConfigureAwait(false),
            cancellationToken
        )
        .ContinueWith(
            t => logger.LogError(t.Exception, "Command server error"),
            cancellationToken: default,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default
        );

        _disposables.Add(commandServer);

        _disposables.Add(_entrypointScheduler.Subscribe(entryPointActivityExecutor));
        _disposables.Add(_activityScheduler.Subscribe(activityExecutor));

        _disposables.Add(_entrypointScheduler);
        _disposables.Add(_activityScheduler);
    }

    private static async Task Run(CancellationToken cancellationToken)
    {
        var configuration = _scope!.Resolve<IHostConfiguration>();

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(configuration.EntrypointActivityQueuePeriod, cancellationToken).ConfigureAwait(false);

            _entrypointScheduler?.QueueActivity(
                new ActivityDescriptor("Main:Entry", typeof(EntryPointActivity), new Common.ActivityContext(Guid.NewGuid().ToString("N")), None)
            );
        }
    }

    private static void ShutDown()
    {
        lock (_mutex)
        {
            if (_disposed) { return; }

            _logger?.LogInformation("Exiting...");

            _cts?.Cancel();

            foreach (var disposable in _disposables) { disposable.Dispose(); }

            _logger?.LogInformation("ShutDown OK");
            _loggerFactory?.Dispose();
            _cts?.Dispose();
            _scope?.Dispose();

            _disposed = true;

            if (Debugger.IsAttached)
            {
                Environment.Exit(1);
            }
        }
    }

    private static void Exit(object? sender, ConsoleCancelEventArgs e) => ShutDown();

    private static IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();

        var configuration = new HostConfiguration();
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = _loggerFactory.CreateLogger<Program>();

        var telemetryCollectorConfig = new TelemetryCollectorConfig(
            new Uri(configuration.PrometheusUri),
            TimeSpan.FromMilliseconds(60000),
            new Uri(configuration.JaegerUri),
            TimeSpan.FromMilliseconds(60000));

        builder.RegisterInstance(_logger).As<ILogger>().SingleInstance();
        builder.RegisterInstance(new TelemetryCollector(telemetryCollectorConfig)).As<ITelemetryCollector>().SingleInstance();
        builder.RegisterType<WorkItemSource>().As<IWorkItemSource>().SingleInstance();
        builder.RegisterType<CommandServer>().SingleInstance();

        builder.RegisterInstance(configuration)
            .As<IHostConfiguration>()
            .As<IHostConfigurationUpdater>()
            .As<IActivityConfiguration>()
            .SingleInstance();

        // TODO: register all activities automatically through type finder
        builder.RegisterType<EntryPointActivity>();
        builder.RegisterType<Library1Activity>();
        builder.RegisterType<Library1OperationA>();
        builder.RegisterType<Library1OperationB>();
        builder.RegisterType<Library1OperationC>();
        builder.RegisterType<Library1OperationD>();
        builder.RegisterType<Library2Activity>();
        builder.RegisterType<Library2OperationA>();
        builder.RegisterType<Library2OperationB>();

        return builder.Build();
    }

    private static IProcessFlowJobActivity ResolveActivity(IContainer scope, ActivityDescriptor descriptor)
    {
        if (descriptor.ActivityType == typeof(EntryPointActivity))
        {
            return scope.Resolve<EntryPointActivity>(
                new NamedParameter("uid", descriptor.ActivityUid),
                new NamedParameter("scheduler", _activityScheduler)
            );
        }

        if (null != descriptor.ActivityType.BaseType
            && descriptor.ActivityType.BaseType.GetGenericTypeDefinition().IsAssignableFrom(typeof(WorkItemsProcessor<>)))
        {
            return descriptor.WorkItemsBatchDescriptor.Match(
                () => throw new InvalidOperationException(),
                wibDescriptor => (scope.Resolve(
                            descriptor.ActivityType,
                            new NamedParameter("uid", descriptor.ActivityUid),
                            new NamedParameter("scheduler", _activityScheduler),
                            new NamedParameter("workItemsBatchDescriptor", wibDescriptor)) as IProcessFlowJobActivity)!
            );
        }

        return (scope.Resolve(descriptor.ActivityType,
                    new NamedParameter("uid", descriptor.ActivityUid),
                    new NamedParameter("scheduler", _activityScheduler)
                ) as IProcessFlowJobActivity)!;
    }
}
