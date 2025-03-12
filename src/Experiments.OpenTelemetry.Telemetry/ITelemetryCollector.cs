using System.Diagnostics;

namespace Experiments.OpenTelemetry.Telemetry;

public interface ITelemetryCollector
{
    #region Metrics

    public void IncrementExecutingActivityCounter(string activityUid);

    public void DecrementExecutingActivityCounter(string activityUid);

    public void IncrementActivityErrorCounter(string activityUid, string errorType);

    public void UpdateActivityQueueLength(string activityUid, long length);

    public void RecordActivityExecutionTime(string activityUid, TimeSpan executionTime);

    public void IncrementWorkItemsQueueCounter(string workItemSourceType, long delta);

    public void DecrementWorkItemsQueueCounter(string workItemSourceType, long delta);

    public void IncrementWorkItemsProcessedCounter(string workItemSourceType, long delta);

    #endregion

    #region Trace

    Activity? StartActivity(string name, string correlationId, string? parentId = null);

    #endregion
}
