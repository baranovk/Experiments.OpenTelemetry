using System.Diagnostics;
using Experiments.OpenTelemetry.Telemetry.Resources;

namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector
{
    #region Public Methods

    public void IncrementExecutingActivityCounter(string activityUid) => UpdateActivityCounter(Counters.ExecutingActivities, 1, activityUid);

    public void DecrementExecutingActivityCounter(string activityUid) => UpdateActivityCounter(Counters.ExecutingActivities, -1, activityUid);

    public void IncrementActivityErrorCounter(string activityUid, string errorType)
        => IncrementActivityCounter(Counters.ActivityErrors, 1, activityUid, errorType);

    public void UpdateActivityQueueLength(string activityUid, long length) => UpdateActivityGauge(Gauges.ActivityQueueLength, length, activityUid);

    public void RecordActivityExecutionTime(string activityUid, TimeSpan executionTime)
        => UpdateActivityHistogram(Histograms.ActivityExecutionTime, (long)executionTime.TotalSeconds, activityUid);

    public void IncrementWorkItemsQueueCounter(string workItemSourceType, long delta)
        => UpdateCounter(Counters.WorkItemsQueueLength, Math.Abs(delta), new KeyValuePair<string, object?>(Tags.WorkItemSourceType, workItemSourceType));

    public void DecrementWorkItemsQueueCounter(string workItemSourceType, long delta)
        => UpdateCounter(Counters.WorkItemsQueueLength, -1 * Math.Abs(delta),
                new KeyValuePair<string, object?>(Tags.WorkItemSourceType, workItemSourceType));

    public void IncrementWorkItemsProcessedCounter(string workItemSourceType, long delta)
        => UpdateCounter(Counters.WorkItemsProcessed, Math.Abs(delta),
                new KeyValuePair<string, object?>(Tags.WorkItemSourceType, workItemSourceType));

    #endregion

    #region Private Methods

    #region Counters

    private void UpdateActivityCounter(string counterUid, long delta, string activityUid)
        => UpdateCounter(counterUid, delta, new KeyValuePair<string, object?>(Tags.ActivityUid, activityUid));

    private void IncrementActivityCounter(string counterUid, long delta, string activityUid, string errorType)
        => IncrementCounter(counterUid, delta, new KeyValuePair<string, object?>(Tags.ActivityUid, activityUid),
                new KeyValuePair<string, object?>(Tags.ErrorType, errorType));

    private void UpdateCounter(string counterUid, long delta, params KeyValuePair<string, object?>[] tags)
    {
        if (_upDownCounters.TryGetValue(counterUid, out var counter))
        {
            counter.Add(delta, tags);
            return;
        }

        throw new InvalidOperationException($"Counter \"{counterUid}\" was not found");
    }

    private void IncrementCounter(string counterUid, long delta, params KeyValuePair<string, object?>[] tags)
    {
        if (0 >= delta) { throw new ArgumentException($"Parameter {delta} should be greater than zero", nameof(delta)); }

        if (_counters.TryGetValue(counterUid, out var counter))
        {
            counter.Add(delta, tags);
            return;
        }

        throw new InvalidOperationException($"Counter \"{counterUid}\" was not found");
    }

    #endregion

    #region Gauges

    private void UpdateActivityGauge(string gaugeUid, long value, string activityUid)
        => UpdateGauge(gaugeUid, value, new KeyValuePair<string, object?>(Tags.ActivityUid, activityUid));

    private void UpdateGauge(string gaugeUid, long value, params KeyValuePair<string, object?>[] tags)
    {
        if (_gauges.TryGetValue(gaugeUid, out var gauge))
        {
            gauge.Record(value, tags);
            return;
        }

        throw new InvalidOperationException($"Gauge \"{gaugeUid}\" was not found");
    }

    #endregion

    #region Histograms

    private void UpdateActivityHistogram(string histogramUid, long value, string activityUid)
        => UpdateHistogram(histogramUid, value, new KeyValuePair<string, object?>(Tags.ActivityUid, activityUid));

    private void UpdateHistogram(string histogramUid, long value, params KeyValuePair<string, object?>[] tags)
    {
        if (_histograms.TryGetValue(histogramUid, out var histogram))
        {
            histogram.Record(value, tags);
            return;
        }

        throw new InvalidOperationException($"Histogram \"{histogramUid}\" was not found");
    }

    #endregion

    #endregion
}
