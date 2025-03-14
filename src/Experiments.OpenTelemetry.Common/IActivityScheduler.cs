using Experiments.OpenTelemetry.Domain;
using Functional;

namespace Experiments.OpenTelemetry.Common;

public sealed record ActivityDescriptor(
    string ActivityUid,
    Type ActivityType,
    ActivityContext Context,
    Option<WorkItemsBatchDescriptor> WorkItemsBatchDescriptor);

public sealed record WorkItemsBatchDescriptor(WorkItemSourceType SourceType, Guid Uid);

public interface IActivityScheduler
{
    void QueueActivity(ActivityDescriptor descriptor);
}
