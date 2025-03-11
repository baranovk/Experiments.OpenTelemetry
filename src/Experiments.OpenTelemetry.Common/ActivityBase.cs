using System.Diagnostics;
using Experiments.OpenTelemetry.Telemetry;
using Functional;
using Microsoft.Extensions.Logging;
using static Functional.F;
using Unit = System.ValueTuple;

namespace Experiments.OpenTelemetry.Common;

public abstract class ActivityBase(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration) : IProcessFlowJobActivity
{
    #region Constants

    private const string ParentTraceIdActivityKey = "PTID";

    #endregion

    #region Fields

    private Activity? _telemetryActivity;
    private static readonly object _mutex = new();
    private static readonly Dictionary<Type, long> _activityExecutionCount = new();

    #endregion

    #region Properties

    public string Uid => uid;

    public IActivityScheduler Scheduler { get; private set; } = scheduler;

    protected ILogger Logger { get; private set; } = logger;

    protected ITelemetryCollector TelemetryCollector => telemetryCollector;

    protected IActivityConfiguration Configuration => configuration;

    #endregion

    #region Public Methods

    public async Task ExecuteAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        var sw = new Stopwatch();
        _telemetryActivity = StartTracingActivity(ctx);

        await (await GetActivityPrologue(ctx, sw)
                .Bind<Exceptional<Unit>, Exceptional<Unit>>(_ => next => GetExecuteActivityMiddleware(ctx, cancellationToken)(next))
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

    protected static ActivityContext CaptureContext(ActivityContext current)
    {
        var ctx = current.Clone();
        ctx.Bag.Set(ParentTraceIdActivityKey, GetParentTracingActivityId(current));
        return ctx;
    }

    protected Task QueueNextActivity<TActivity>(string activityUid, ActivityContext ctx, Option<Guid> workItemBatchUid,
        CancellationToken cancellationToken = default)
        => QueueNextActivity(activityUid, typeof(TActivity), ctx, workItemBatchUid, cancellationToken);

    protected Task QueueNextActivity(string activityUid, Type activityType, ActivityContext ctx, Option<Guid> workItemBatchUid,
        CancellationToken cancellationToken = default)
    {
        Scheduler.QueueActivity(new ActivityDescriptor(activityUid, activityType, CaptureContext(ctx), workItemBatchUid));
        return Task.CompletedTask;
    }

    protected Activity? StartTracingActivity(ActivityContext ctx, string? activityName = null)
        => TelemetryCollector.StartActivity(activityName ?? $"Execute {Uid}", ctx.CorrelationId, GetParentTracingActivityId(ctx));

    protected static long IncrementExecutionCount(Type activityType)
    {
        lock (_mutex)
        {
            _activityExecutionCount[activityType] =
                _activityExecutionCount.TryGetValue(activityType, out var count) ? count + 1 : 1;

            return _activityExecutionCount[activityType];
        }
    }

    #endregion

    #region Private Methods

    #region Activity Flow

    private async Task<dynamic> ExecuteActivityPrologue(ActivityContext ctx, Stopwatch swActivityExecutionTime, Func<Exceptional<Unit>, Task<dynamic>> next)
    {
        swActivityExecutionTime.Start();

        Logger.LogInformation("Activity {Uid} has started", Uid);
        TelemetryCollector.IncrementExecutingActivityCounter(Uid);

        return await next(new Unit()).ConfigureAwait(false);
    }

    private async Task<dynamic> ExecuteActivityAsync(ActivityContext ctx, CancellationToken cancellationToken,
        Func<Exceptional<Unit>, Task<dynamic>> next)
        => await (await TryAsync(() => ExecuteInternalAsync(ctx, cancellationToken)).RunAsync().ConfigureAwait(false))
                .Match(
                    ex => Async<dynamic>(ex),
                    u => next(Exceptional(u))
                ).ConfigureAwait(false);

    private Task ExecuteActivityEpilogue(Stopwatch swActivityExecutionTime, Option<Exception> error)
    {
        swActivityExecutionTime.Stop();
        Logger.LogInformation("{Uid} has finished. Execution time: {ExecutionTime}", Uid, swActivityExecutionTime.Elapsed);

        TelemetryCollector.DecrementExecutingActivityCounter(Uid);
        TelemetryCollector.RecordActivityExecutionTime(Uid, TimeSpan.FromMilliseconds(swActivityExecutionTime.ElapsedMilliseconds));

        return error.Match(
            () => { _telemetryActivity?.Stop(); return Task.CompletedTask; },
            ex =>
            {
                TelemetryCollector.IncrementActivityErrorCounter(Uid, ex is DomainException domEx ? domEx.ErrorType.ToString() : "Unclassified");
                Logger.LogError(ex, "Execute activity error");
                _telemetryActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _telemetryActivity?.Stop();
                return Task.CompletedTask;
            }
        );
    }

    #endregion

    #region Utility

    private AsyncMiddleware<Exceptional<Unit>> GetActivityPrologue(ActivityContext ctx, Stopwatch swActivityExecutionTime)
    => new(new Func<ActivityContext, Stopwatch, Func<Exceptional<Unit>, Task<dynamic>>, Task<dynamic>>(ExecuteActivityPrologue)
        .Curry()(ctx)(swActivityExecutionTime));

    private AsyncMiddleware<Exceptional<Unit>> GetExecuteActivityMiddleware(ActivityContext ctx, CancellationToken cancellationToken)
        => new(new Func<ActivityContext, CancellationToken, Func<Exceptional<Unit>, Task<dynamic>>, Task<dynamic>>(ExecuteActivityAsync)
                .Curry()(ctx)(cancellationToken));

    private static string? GetParentTracingActivityId(ActivityContext ctx)
        => Activity.Current?.SpanId.ToString() ?? ctx.Bag.Get(ParentTraceIdActivityKey)?.ToString();

    #endregion

    #endregion
}
