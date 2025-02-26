namespace Experiments.OpenTelemetry.Common;

public interface IWorkItemSource
{
    Task<Guid> QueueWorkItemsAsync(WorkItemSourceType sourceType, IEnumerable<WorkItem> workItems, CancellationToken cancellationToken = default);

    Task<IEnumerable<WorkItem>> GetWorkItemsAsync(WorkItemSourceType sourceType, Guid batchUid, CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(WorkItemSourceType sourceType, Guid batchUid, CancellationToken cancellationToken = default);
}

public enum WorkItemSourceType
{
    Type1,
    Type2
}
