using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using Experiments.OpenTelemetry.Common;
using Functional;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Host;

internal sealed class ActivityExecutor(
    int maxConcurrentActivitiesCount,
    Func<ActivityDescriptor, IProcessFlowJobActivity> buildActivity,
    ILogger logger,
    CancellationToken cancellationToken = default)
    : IObserver<ActivityDescriptor>, IDisposable
{
    private bool _disposed;
    private static readonly object _mutex = new();
    private readonly StringBuilder _sb = new();
    private readonly SemaphoreSlim _activitySemaphore = new(maxConcurrentActivitiesCount, maxConcurrentActivitiesCount);
    private readonly Func<ActivityDescriptor, IProcessFlowJobActivity> _buildActivity = buildActivity;
    private readonly ILogger _logger = logger;
    private readonly CancellationToken _cancellationToken = cancellationToken;
    private readonly ConcurrentDictionary<string, long> _activityCounters = [];

    public void OnNext(ActivityDescriptor value)
    {
        if (_disposed) { return; }

        var ctx = new ActivityContext(value.CorrelationId);
        var activity = _buildActivity(value);

        Task.Run(
            async () =>
            {
                await _activitySemaphore.WaitAsync(_cancellationToken).ConfigureAwait(false);

                lock (_mutex)
                {
                    UpdateActivityCounter(activity.Uid);
                    LogActiveActivityCounter($"[{DateTime.Now.ToString("hh:mm:ss.fff tt")}] Before execute {activity.Uid}");
                }

                await activity!.ExecuteAsync(ctx, _cancellationToken).ConfigureAwait(false);
            },
            _cancellationToken
        )
        .ContinueWith(
            t =>
            {
                lock (_mutex)
                {
                    UpdateActivityCounter(activity.Uid, -1);
                    LogActiveActivityCounter($"[{DateTime.Now.ToString("hh:mm:ss.fff tt")}] After execute {activity.Uid}");
                }

                _activitySemaphore.Release();
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
    private void LogActiveActivityCounter(string label)
        => _activityCounters.Keys
                .Aggregate(
                    (TotalActivities: 0L, Sb: _sb.Clear()),
                    (acc, key) => { acc.Sb.Append($"{key}: {_activityCounters[key]}\r\n"); acc.TotalActivities += _activityCounters[key]; return acc; }
                )
                .Pipe(
                    acc => _logger.LogInformation("{Label}. Execution activities count:\n {ActivitiesSummary}\nTotal executiong activities: {TotalExecutingActivities}",
                    label,
                    acc.Sb.ToString(),
                    acc.TotalActivities
                ));

    public void Dispose()
    {
        if (_disposed) { return; }
        _activitySemaphore?.Dispose();
        _disposed = true;
    }
}
