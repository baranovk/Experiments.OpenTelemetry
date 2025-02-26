using System.Diagnostics;
using Experiments.OpenTelemetry.Telemetry;
using Functional;
using Microsoft.Extensions.Logging;
using static Functional.F;
using Unit = System.ValueTuple;

namespace Experiments.OpenTelemetry.Common;

public abstract class ActivityBase(string uid, ILogger logger, IActivityScheduler scheduler) : IProcessFlowJobActivity
{
    #region Properties

    public string Uid => uid;

    public IActivityScheduler Scheduler { get; private set; } = scheduler;

    protected ILogger Logger { get; private set; } = logger;

    protected TelemetryCollector? TelemetryCollector { get; private set; }

    #endregion

    #region Public Methods

    public async Task ExecuteAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        var sw = new Stopwatch();

        await (await GetActivityPrologue(ctx, sw)
                .Bind<Exceptional<Unit>, Exceptional<Unit>>(_ => next => IncrementExecutingActivityCounter(next))
                .Bind<Exceptional<Unit>, Exceptional<Unit>>(_ => next => GetExecuteActivityMiddleware(ctx, cancellationToken)(next))
                .Bind<Exceptional<Unit>, Exceptional<Unit>>(_ => next => DecrementExecutingActivityCounter(next))
                .RunAsync()
                .ConfigureAwait(false)
              )
              .Match(
                   async ex => await ExecuteActivityEpilogue(sw, ex).ConfigureAwait(false),
                   async _ => await ExecuteActivityEpilogue(sw, None).ConfigureAwait(false)
               )
              .ConfigureAwait(false);
    }

    #endregion

    #region Protected Methods

    protected abstract Task<Unit> ExecuteInternalAsync(ActivityContext ctx, CancellationToken cancellationToken = default);

    #endregion

    #region Private Methods

    private AsyncMiddleware<Exceptional<Unit>> GetActivityPrologue(ActivityContext ctx, Stopwatch swActivityExecutionTime)
        => new(new Func<ActivityContext, Stopwatch, Func<Exceptional<Unit>, Task<dynamic>>, Task<dynamic>>(ExecuteActivityPrologue)
            .Curry()(ctx)(swActivityExecutionTime));

    private AsyncMiddleware<Exceptional<Unit>> GetExecuteActivityMiddleware(ActivityContext ctx, CancellationToken cancellationToken)
        => new(new Func<ActivityContext, CancellationToken, Func<Exceptional<Unit>, Task<dynamic>>, Task<dynamic>>(ExecuteActivityAsync)
                .Curry()(ctx)(cancellationToken));

    private async Task<dynamic> ExecuteActivityPrologue(ActivityContext ctx, Stopwatch swActivityExecutionTime, Func<Exceptional<Unit>, Task<dynamic>> next)
    {
        swActivityExecutionTime.Start();
        Logger.LogInformation("Activity {Uid} has started", Uid);

        TelemetryCollector = TelemetryCollector.GetInstance(ctx.TelemetryCollectorConfig);
        return await next(new Unit()).ConfigureAwait(false);
    }

    private async Task<dynamic> IncrementExecutingActivityCounter(Func<Exceptional<Unit>, Task<dynamic>> next)
    {
        TelemetryCollector?.IncrementExecutingActivityCounter(Uid);
        return await next(Exceptional(new Unit())).ConfigureAwait(false);
    }

    private async Task<dynamic> ExecuteActivityAsync(ActivityContext ctx, CancellationToken cancellationToken,
        Func<Exceptional<Unit>, Task<dynamic>> next)
        => await (await TryAsync(() => ExecuteInternalAsync(ctx, cancellationToken)).RunAsync().ConfigureAwait(false))
            .Match(
                ex => Async<dynamic>(Exceptional(ex)),
                u => next(Exceptional(u))
            )
            .ConfigureAwait(false);

    private async Task<dynamic> DecrementExecutingActivityCounter(Func<Exceptional<Unit>, Task<dynamic>> next)
    {
        TelemetryCollector?.DecrementExecutingActivityCounter(Uid);
        return await next(new Unit()).ConfigureAwait(false);
    }

    private Task ExecuteActivityEpilogue(Stopwatch swActivityExecutionTime, Option<Exception> error)
    {
        // TODO: send sw.TotalSeconds to histogram
        swActivityExecutionTime.Stop();
        Logger.LogInformation("{Uid} has finished. Execution time: {ExecutionTime}", Uid, swActivityExecutionTime.Elapsed);

        return error.Match(
            () => Task.CompletedTask,
            ex =>
            {
                TelemetryCollector?.IncrementActivityErrorCounter(Uid);
                Logger.LogError(ex, "Execute activity error");
                return Task.CompletedTask;
            }
        );
    }

    #endregion
}
