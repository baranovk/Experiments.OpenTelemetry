using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;

namespace Experiments.OpenTelemetry.Host;

internal sealed class WorkItemSource : IWorkItemSource
{
    private readonly ITelemetryCollector _telemetryCollector;

    private readonly Dictionary<WorkItemSourceType, Dictionary<Guid, IEnumerable<WorkItem>>> _storage = new()
    {
        { WorkItemSourceType.Type1, new Dictionary<Guid, IEnumerable<WorkItem>>() },
        { WorkItemSourceType.Type2, new Dictionary<Guid, IEnumerable<WorkItem>>() }
    };

    public WorkItemSource(ITelemetryCollector telemetryCollector)
    {
        _telemetryCollector = telemetryCollector;
    }

    public Task<IEnumerable<WorkItem>> GetWorkItemsAsync(WorkItemSourceType sourceType, Guid batchUid, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage[sourceType].TryGetValue(batchUid, out var workItems) ? workItems : []);
    }

    public Task MarkProcessedAsync(WorkItemSourceType sourceType, Guid batchUid, CancellationToken cancellationToken = default)
    {
        if (_storage.TryGetValue(sourceType, out var workItemsSegment))
        {
            if (!workItemsSegment.TryGetValue(batchUid, out var workItems))
            {
                return Task.CompletedTask;
            }

            var count = workItems.Count();
            workItemsSegment.Remove(batchUid);
            _telemetryCollector.DecrementWorkItemsQueueCounter(sourceType.ToString(), count);
        }

        return Task.CompletedTask;
    }

    public Task<Guid> QueueWorkItemsAsync(WorkItemSourceType sourceType, IEnumerable<WorkItem> workItems, CancellationToken cancellationToken = default)
    {
        var key = Guid.NewGuid();

        _storage[sourceType].Add(key, workItems);
        _telemetryCollector.IncrementWorkItemsQueueCounter(sourceType.ToString(), workItems.Count());

        return Task.FromResult(key);
    }
}
