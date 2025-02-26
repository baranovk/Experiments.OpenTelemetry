using System.Runtime.CompilerServices;
using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Host;

internal sealed class ActivityExecutor(
    ILogger logger,
    IActivityScheduler scheduler,
    IWorkItemSource workItemSource,
    TelemetryCollectorConfig telemetryCollectorConfig,
    CancellationToken cancellationToken = default)
    : IObserver<ActivityDescriptor>
{
    private readonly ILogger _logger = logger;
    private readonly IActivityScheduler _scheduler = scheduler;
    private readonly IWorkItemSource _workItemSource = workItemSource;
    private readonly TelemetryCollectorConfig _telemetryCollectorConfig = telemetryCollectorConfig;
    private readonly CancellationToken _cancellationToken = cancellationToken;
    private readonly Dictionary<string, long> _activityCounters = [];

    public void OnNext(ActivityDescriptor value)
    {
        var ctx = new ActivityContext(_telemetryCollectorConfig, value.CorrelationId);
        var activity = CreateActivity(value);

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

    private IProcessFlowJobActivity CreateActivity(ActivityDescriptor descriptor)
    {
        if (descriptor.ActivityType == typeof(EntryPointActivity))
        {
            return (Activator.CreateInstance(descriptor.ActivityType,
                descriptor.ActivityUid, _logger, _scheduler, _workItemSource) as IProcessFlowJobActivity)!;
        }

        if (descriptor.ActivityType.BaseType == typeof(WorkItemsProcessor))
        {
            descriptor.WorkItemsBatchUid.Match(
                () => throw new InvalidOperationException(),
                uid => (Activator.CreateInstance(descriptor.ActivityType,
                    descriptor.ActivityUid, _logger, _scheduler, descriptor.WorkItemsBatchUid, _workItemSource, uid) as IProcessFlowJobActivity)!
            );
        }

        return (Activator.CreateInstance(descriptor.ActivityType, descriptor.ActivityUid, _logger, _scheduler) as IProcessFlowJobActivity)!;
    }
}
