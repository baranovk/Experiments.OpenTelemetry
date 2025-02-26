using System.Diagnostics.Metrics;
using Experiments.OpenTelemetry.Telemetry.Resources;

namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector
{
    #region Public Methods

    public void IncrementExecutingActivityCounter(string activityUid) => UpdateActivityCounter(Counters.ExecutingActivities, 1, activityUid);

    public void DecrementExecutingActivityCounter(string activityUid) => UpdateActivityCounter(Counters.ExecutingActivities, -1, activityUid);

    public void IncrementActivityErrorCounter(string activityUid) => IncrementActivityCounter(Counters.ActivityErrors, 1, activityUid);

    public void UpdateActivityQueueLength(string activityUid, long length) => UpdateActivityGauge(Gauges.ActivityQueueLength, length, activityUid);

    #endregion

    #region Private Methods

    private void UpdateActivityCounter(string counterUid, long delta, string activityUid)
        => UpdateCounter(counterUid, delta, new KeyValuePair<string, object?>(Tags.ActivityUid, activityUid));

    private void IncrementActivityCounter(string counterUid, long delta, string activityUid)
        => IncrementCounter(counterUid, delta, new KeyValuePair<string, object?>(Tags.ActivityUid, activityUid));

    private void UpdateCounter(string counterUid, long delta, params KeyValuePair<string, object?>[] tags)
    {
        if (_upDownCounters.TryGetValue(counterUid, out var counter))
        {
            counter.Add(delta, tags);
            return;
        }

        throw new InvalidOperationException($"Counter {counterUid} was not found");
    }

    private void IncrementCounter(string counterUid, long delta, params KeyValuePair<string, object?>[] tags)
    {
        if (0 >= delta) { throw new ArgumentException($"Parameter {delta} should be greater than zero", nameof(delta)); }

        if (_counters.TryGetValue(counterUid, out var counter))
        {
            counter.Add(delta, tags);
            return;
        }

        throw new InvalidOperationException($"Counter {counterUid} was not found");
    }

    private void UpdateActivityGauge(string gaugeUid, long value, string activityUid)
        => UpdateGauge(gaugeUid, value, new KeyValuePair<string, object?>(Tags.ActivityUid, activityUid));

    private void UpdateGauge(string gaugeUid, long value, params KeyValuePair<string, object?>[] tags)
    {
        if (_gauges.TryGetValue(gaugeUid, out var gauge))
        {
            gauge.Record(value, tags);
            return;
        }

        throw new InvalidOperationException($"Gauge {gaugeUid} was not found");
    }

    #endregion
}
