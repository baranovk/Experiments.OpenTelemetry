namespace Experiments.OpenTelemetry.Common;

public interface IActivityConfiguration
{
    int ErrorRatePercent { get; }

    int ActivityExecutionTimeMinMilliseconds { get; }

    int ActivityExecutionTimeMaxMilliseconds { get; }

    int ActivityWorkItemProcessingTimeMinMilliseconds { get; }

    int ActivityWorkItemProcessingTimeMaxMilliseconds { get; }
}
