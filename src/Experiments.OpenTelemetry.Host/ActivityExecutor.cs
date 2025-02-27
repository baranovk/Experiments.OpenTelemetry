using System.Runtime.CompilerServices;
using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Host;

internal sealed class ActivityExecutor(
    int maxConcurrentActivitiesCount,
    Func<ActivityDescriptor, IProcessFlowJobActivity> buildActivity,
    ILogger logger,
    CancellationToken cancellationToken = default)
    : IObserver<ActivityDescriptor>, IDisposable
{
    private bool _disposed;
    private readonly SemaphoreSlim _activitySemaphore = new(maxConcurrentActivitiesCount, maxConcurrentActivitiesCount);
    private readonly Func<ActivityDescriptor, IProcessFlowJobActivity> _buildActivity = buildActivity;
    private readonly ILogger _logger = logger;
    private readonly CancellationToken _cancellationToken = cancellationToken;
    private readonly Dictionary<string, long> _activityCounters = [];

    public void OnNext(ActivityDescriptor value)
    {
        _activitySemaphore.Wait();

        var ctx = new ActivityContext(value.CorrelationId);
        var activity = _buildActivity(value);

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
                _activitySemaphore.Release();
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

    public void OnCompleted() { }

    public void OnError(Exception error) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateActivityCounter(string activityUid, long delta = 1)
        => _activityCounters[activityUid] = _activityCounters.TryGetValue(activityUid, out var counter) ? counter + delta : delta;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogActiveActivityCounter(string activityUid)
        => _logger.LogInformation("Active {ActivityUid} activity count: {ActivityCount}", activityUid, _activityCounters[activityUid]);

    public void Dispose()
    {
        if (_disposed) { return; }
        _activitySemaphore?.Dispose();
        _disposed = true;
    }
}
