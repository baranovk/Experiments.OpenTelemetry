namespace Experiments.OpenTelemetry.Telemetry;

public interface ITelemetryCollector
{
    public void IncrementExecutingActivityCounter(string activityUid);

    public void DecrementExecutingActivityCounter(string activityUid);

    public void IncrementActivityErrorCounter(string activityUid);

    public void UpdateActivityQueueLength(string activityUid, long length);

    public void RecordActivityExecutionTime(string activityUid, TimeSpan executionTime);
}
