using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using Experiments.OpenTelemetry.Common;
using Functional;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Host;

internal sealed class ActivityExecutor(
    Func<int> getMaxConcurrentActivitiesCount,
    Func<ActivityDescriptor, IProcessFlowJobActivity> buildActivity,
    ILogger logger,
    CancellationToken cancellationToken = default)
    : IObserver<ActivityDescriptor>
{
    private int _executingActivityCount;
    private static readonly object _mutex = new();
    private static readonly object _parallelismMutex = new();
    private readonly StringBuilder _sb = new();
    private readonly ILogger _logger = logger;
    private readonly CancellationToken _cancellationToken = cancellationToken;
    private readonly ConcurrentDictionary<string, long> _activityCounters = [];
    private readonly Func<int> _getMaxConcurrentActivitiesCount = getMaxConcurrentActivitiesCount;
    private readonly Func<ActivityDescriptor, IProcessFlowJobActivity> _buildActivity = buildActivity;

    public void OnNext(ActivityDescriptor value)
    {
        var activity = _buildActivity(value);

        Task.Run(
            async () =>
            {
                lock (_parallelismMutex)
                {
                    if (_executingActivityCount == _getMaxConcurrentActivitiesCount())
                    {
                        _logger.LogInformation("Max execution activities limit was reached: {MaxConcurrentActivities}", _executingActivityCount);
                        Monitor.Wait(_parallelismMutex);
                    }

                    _executingActivityCount++;
                }

                lock (_mutex)
                {
                    UpdateActivityCounter(activity.Uid);
                    LogActiveActivityCounter($"[{DateTime.Now.ToString("hh:mm:ss.fff tt")}] Before execute {activity.Uid}");
                }

                await activity!.ExecuteAsync(value.Context, _cancellationToken).ConfigureAwait(false);
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

                lock (_parallelismMutex)
                {
                    _executingActivityCount--;
                    Monitor.Pulse(_parallelismMutex);
                }

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
        => _activityCounters[activityUid] = _activityCounters.TryGetValue(activityUid, out var counter)
                                                ? Math.Max(counter + delta, 0) : Math.Max(delta, 0);

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
}
