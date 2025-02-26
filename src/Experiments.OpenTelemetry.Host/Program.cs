using System.Diagnostics;
using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Host;

internal sealed class Program
{
    #region Constants

    private const int ActivityQueuePeriod = 5000;
    private const int ActivityQueueLimit = 100;
    private const string PremetheusUri = "http://localhost:9090/api/v1/otlp/v1/metrics";

    #endregion

    #region Fields

    private static bool _disposed;
    private static ActivityScheduler? _entrypointScheduler;
    private static ActivityScheduler? _activityScheduler;
    private static CancellationTokenSource? _cts;
    private static TelemetryCollector? _telemetryCollector;
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _logger;

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

        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = _loggerFactory.CreateLogger<Program>();

        static void updateActivityQueueLength(string activityUid, int activityQueueLength)
            => _telemetryCollector?.UpdateActivityQueueLength(activityUid, activityQueueLength);

        _entrypointScheduler = new ActivityScheduler(
            _logger,
            ActivityQueueLimit,
            updateActivityQueueLength,
            cancellationToken
        );

        _activityScheduler = new ActivityScheduler(
            _logger,
            ActivityQueueLimit,
            updateActivityQueueLength,
            cancellationToken
        );

        var telemetryCollectorConfig = new TelemetryCollectorConfig(new Uri(PremetheusUri), TimeSpan.FromMilliseconds(3000));
        _telemetryCollector = TelemetryCollector.GetInstance(telemetryCollectorConfig);

        _entrypointScheduler.Subscribe(new ActivityExecutor(_logger, _activityScheduler, telemetryCollectorConfig, cancellationToken));
        _activityScheduler.Subscribe(new ActivityExecutor(_logger, _activityScheduler, telemetryCollectorConfig, cancellationToken));
    }

    private static async Task Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(ActivityQueuePeriod, cancellationToken).ConfigureAwait(false);

            _entrypointScheduler?.QueueActivity(
                new ActivityDescriptor("Main_EntryPoint_Activity", typeof(EntryPointActivity), Guid.NewGuid().ToString())
            );
        }
    }

    private static void ShutDown()
    {
        if (_disposed) { return; }

        _logger?.LogInformation("Exiting...");

        _cts?.Cancel();

        _entrypointScheduler?.Dispose();
        _activityScheduler?.Dispose();

        _logger?.LogInformation("ShutDown OK");
        _loggerFactory?.Dispose();
        _cts?.Dispose();

        _disposed = true;

        if (Debugger.IsAttached)
        {
            Environment.Exit(1);
        }
    }

    private static void Exit(object? sender, ConsoleCancelEventArgs e) => ShutDown();
}
