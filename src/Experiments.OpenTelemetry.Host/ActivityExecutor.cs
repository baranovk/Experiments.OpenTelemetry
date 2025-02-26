using System.Runtime.CompilerServices;
using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Host;

internal sealed class ActivityExecutor(
    ILogger logger,
    IActivityScheduler scheduler,
    TelemetryCollectorConfig telemetryCollectorConfig,
    CancellationToken cancellationToken = default)
    : IObserver<ActivityDescriptor>
{
    private readonly ILogger _logger = logger;
    private readonly IActivityScheduler _scheduler = scheduler;
    private readonly TelemetryCollectorConfig _telemetryCollectorConfig = telemetryCollectorConfig;
    private readonly CancellationToken _cancellationToken = cancellationToken;
    private readonly Dictionary<string, long> _activityCounters = [];

    public void OnNext(ActivityDescriptor value)
    {
        var ctx = new ActivityContext(_telemetryCollectorConfig, value.CorrelationId);
        var activity = (Activator.CreateInstance(value.ActivityType, value.ActivityUid, _logger, _scheduler) as IProcessFlowJobActivity)!;

        UpdateActivityCounter(activity.Uid);
        LogActiveActivityCounter(activity.Uid);

        Task.Factory.StartNew(
            async () => await activity!.ExecuteAsync(ctx, _cancellationToken).ConfigureAwait(false),
            _cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        )
        .ContinueWith(
            t =>
            {
                UpdateActivityCounter(activity.Uid, -1);
                LogActiveActivityCounter(activity.Uid);
                return t;
            },
            cancellationToken: default, TaskContinuationOptions.None, TaskScheduler.Default
        )
        .ContinueWith(
            t => _logger.LogError(t.Exception, "Error while executing activity"),
            default, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default
        );
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateActivityCounter(string activityUid, long delta = 1)
        => _activityCounters[activityUid] = _activityCounters.TryGetValue(activityUid, out var counter) ? counter + delta : delta;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogActiveActivityCounter(string activityUid)
        => _logger.LogInformation("Active {ActivityUid} activity count: {ActivityCount}", activityUid, _activityCounters[activityUid]);
}
