using Experiments.OpenTelemetry.Common;

namespace Experiments.OpenTelemetry.Host;

internal sealed class WorkItemSource : IWorkItemSource
{
    private readonly Dictionary<WorkItemSourceType, Dictionary<Guid, IEnumerable<WorkItem>>> _storage = new()
    {
        { WorkItemSourceType.Type1, new Dictionary<Guid, IEnumerable<WorkItem>>() },
        { WorkItemSourceType.Type2, new Dictionary<Guid, IEnumerable<WorkItem>>() }
    };

    public Task<IEnumerable<WorkItem>> GetWorkItemsAsync(WorkItemSourceType sourceType, Guid batchUid, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage[sourceType].TryGetValue(batchUid, out var workItems) ? workItems : []);
    }

    public Task MarkProcessedAsync(WorkItemSourceType sourceType, Guid batchUid, CancellationToken cancellationToken = default)
    {
        _storage[sourceType].Remove(batchUid);
        return Task.CompletedTask;
    }

    public Task<Guid> QueueWorkItemsAsync(WorkItemSourceType sourceType, IEnumerable<WorkItem> workItems, CancellationToken cancellationToken = default)
    {
        var key = Guid.NewGuid();
        _storage[sourceType].Add(key, workItems);
        return Task.FromResult(key);
    }
}
