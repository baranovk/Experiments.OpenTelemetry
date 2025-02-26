namespace Experiments.OpenTelemetry.Common;

public sealed record ActivityDescriptor(string ActivityUid, Type ActivityType, string CorrelationId);

public interface IActivityScheduler
{
    void QueueActivity(ActivityDescriptor descriptor);
}
