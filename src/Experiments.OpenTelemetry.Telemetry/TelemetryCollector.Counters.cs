using Experiments.OpenTelemetry.Domain;
using Experiments.OpenTelemetry.Telemetry.Resources;

namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector
{
    #region Public Methods

    public void IncrementExecutingActivityCounter(string activityUid, WorkItemSourceType? workItemSourceType = null)
        => UpdateExecutingActivityCounter(activityUid, 1, workItemSourceType);

    public void DecrementExecutingActivityCounter(string activityUid, WorkItemSourceType? workItemSourceType = null)
        => UpdateExecutingActivityCounter(activityUid, -1, workItemSourceType);

    public void IncrementActivityErrorCounter(string activityUid, string errorType, WorkItemSourceType? workItemSourceType = null)
    {
        if (null == workItemSourceType)
        {
            IncrementCounter(Counters.ActivityErrors, 1, activityUid.ToActivityUidTag(), errorType.ToErrorTypeTag());
        }
        else
        {
            IncrementCounter(Counters.ActivityErrors, 1, activityUid.ToActivityUidTag(),
                errorType.ToErrorTypeTag(), workItemSourceType.Value.ToWorkItemSourceTypeTag());
        }
    }

    public void UpdateActivityQueueLength(string activityUid, long length, WorkItemSourceType? workItemSourceType = null)
    {
        if (null == workItemSourceType)
        {
            UpdateGauge(Gauges.ActivityQueueLength, length, activityUid.ToActivityUidTag());
        }
        else
        {
            UpdateGauge(Gauges.ActivityQueueLength, length, activityUid.ToActivityUidTag(), workItemSourceType.Value.ToWorkItemSourceTypeTag());
        }
    }

    public void RecordActivityExecutionTime(string activityUid, TimeSpan executionTime, WorkItemSourceType? workItemSourceType = null)
    {
        if (null == workItemSourceType)
        {
            UpdateHistogram(Histograms.ActivityExecutionTime, (long)executionTime.TotalSeconds, activityUid.ToActivityUidTag());
        }
        else
        {
            UpdateHistogram(Histograms.ActivityExecutionTime, (long)executionTime.TotalSeconds, activityUid.ToActivityUidTag(),
                workItemSourceType.Value.ToWorkItemSourceTypeTag());
        }
    }

    public void IncrementWorkItemsQueueCounter(WorkItemSourceType workItemSourceType, long delta)
        => UpdateCounter(Counters.WorkItemsQueueLength, Math.Abs(delta), workItemSourceType.ToWorkItemSourceTypeTag());

    public void DecrementWorkItemsQueueCounter(WorkItemSourceType workItemSourceType, long delta)
        => UpdateCounter(Counters.WorkItemsQueueLength, -1 * Math.Abs(delta), workItemSourceType.ToWorkItemSourceTypeTag());

    public void IncrementWorkItemsProcessedCounter(WorkItemSourceType workItemSourceType, long delta)
        => UpdateCounter(Counters.WorkItemsProcessed, Math.Abs(delta), workItemSourceType.ToWorkItemSourceTypeTag());

    #endregion

    #region Private Methods

    #region Counters

    public void UpdateExecutingActivityCounter(string activityUid, long delta, WorkItemSourceType? workItemSourceType = null)
    {
        if (null == workItemSourceType)
        {
            UpdateCounter(Counters.ExecutingActivities, delta, activityUid.ToActivityUidTag());
        }
        else
        {
            UpdateCounter(Counters.ExecutingActivities, delta, activityUid.ToActivityUidTag(), workItemSourceType.Value.ToWorkItemSourceTypeTag());
        }
    }

    public void IncrementCounter(string counterUid, string activityUid, string errorType, WorkItemSourceType? workItemSourceType = null)
    {
        if (null == workItemSourceType)
        {
            IncrementCounter(counterUid, 1, activityUid.ToActivityUidTag(), errorType.ToErrorTypeTag());
        }
        else
        {
            IncrementCounter(counterUid, 1, activityUid.ToActivityUidTag(),
                errorType.ToErrorTypeTag(), workItemSourceType.Value.ToWorkItemSourceTypeTag());
        }
    }

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

internal static class TagExtensions
{
    public static KeyValuePair<string, object?> ToActivityUidTag(this string activityUid) => new(Tags.ActivityUid, activityUid);

    public static KeyValuePair<string, object?> ToWorkItemSourceTypeTag(this WorkItemSourceType workItemSourceType)
        => new(Tags.WorkItemSourceType, workItemSourceType);

    public static KeyValuePair<string, object?> ToErrorTypeTag(this string errorType) => new(Tags.ErrorType, errorType);
}
