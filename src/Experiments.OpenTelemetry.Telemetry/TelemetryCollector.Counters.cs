using System.Diagnostics.Metrics;
using Experiments.OpenTelemetry.Telemetry.Resources;

namespace Experiments.OpenTelemetry.Telemetry;
public partial class TelemetryCollector
{
    #region Fields

    //private readonly Dictionary<string, Counter<long>> _counters = [];
    private readonly Dictionary<string, UpDownCounter<long>> _upDownCounters = [];

    #endregion

    #region Public Methods

    public void IncrementActivityQueueCounter(string activityUid) => UpdateActivityQueueCounter(1, activityUid);

    public void DecrementActivityQueueCounter(string activityUid) => UpdateActivityQueueCounter(-1, activityUid);

    public void IncrementExecutingActivityCounter(string activityUid) => UpdateExecutingActivityCounter(1, activityUid);

    public void DecrementExecutingActivityCounter(string activityUid) => UpdateExecutingActivityCounter(-1, activityUid);

    #endregion

    #region Private Methods

    private void UpdateExecutingActivityCounter(long delta, string activityUid)
    {
        var counter = _upDownCounters[Counters.ExecutingActivities];
        counter.Add(delta, new KeyValuePair<string, object?>(Tags.ActivityUid, activityUid));
    }

    private void UpdateActivityQueueCounter(long delta, string activityUid)
    {
        var counter = _upDownCounters[Counters.ActivityQueue];
        counter.Add(delta, new KeyValuePair<string, object?>(Tags.ActivityUid, activityUid));
    }

    #endregion
}
