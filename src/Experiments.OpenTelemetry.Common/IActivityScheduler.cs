using Functional;

namespace Experiments.OpenTelemetry.Common;

public sealed record ActivityDescriptor(string ActivityUid, Type ActivityType, string CorrelationId, Option<Guid> WorkItemsBatchUid);

public interface IActivityScheduler
{
    void QueueActivity(ActivityDescriptor descriptor);
}
