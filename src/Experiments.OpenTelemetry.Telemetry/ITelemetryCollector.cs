using System.Diagnostics;
using Experiments.OpenTelemetry.Domain;

namespace Experiments.OpenTelemetry.Telemetry;

public interface ITelemetryCollector
{
    #region Metrics

    public void IncrementExecutingActivityCounter(string activityUid, WorkItemSourceType? workItemSourceType = null);

    public void DecrementExecutingActivityCounter(string activityUid, WorkItemSourceType? workItemSourceType = null);

    public void IncrementActivityErrorCounter(string activityUid, string errorType, WorkItemSourceType? workItemSourceType = null);

    public void UpdateActivityQueueLength(string activityUid, long length, WorkItemSourceType? workItemSourceType = null);

    public void RecordActivityExecutionTime(string activityUid, TimeSpan executionTime, WorkItemSourceType? workItemSourceType = null);

    public void IncrementWorkItemsQueueCounter(WorkItemSourceType workItemSourceType, long delta);

    public void DecrementWorkItemsQueueCounter(WorkItemSourceType workItemSourceType, long delta);

    public void IncrementWorkItemsProcessedCounter(WorkItemSourceType workItemSourceType, long delta);

    #endregion

    #region Trace

    Activity? StartActivity(string name, string correlationId, string? parentId = null);

    #endregion
}
