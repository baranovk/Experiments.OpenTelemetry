using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Library1;
using Experiments.OpenTelemetry.Library2;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Host;

internal sealed class EntryPointActivity(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector)
    : CommonActivity(uid, logger, scheduler, telemetryCollector)
{
    private readonly IWorkItemSource _workItemSource = workItemSource;

    private static readonly (string ActivityUid, Type ActivityType, WorkItemSourceType SourceType)[] _activityDescriptors =
    [
        ("Lib1:Entry", typeof(Library1Activity), WorkItemSourceType.Type1),
        ("Lib2:Entry", typeof(Library2Activity), WorkItemSourceType.Type2)
    ];

    protected override async Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        var descriptor = _activityDescriptors[new Random().Next(_activityDescriptors.Length)];
        var workItemsBatchUid = await EnqueuWorkItems(descriptor.SourceType, cancellationToken).ConfigureAwait(false);
        Scheduler.QueueActivity(new ActivityDescriptor(descriptor.ActivityUid, descriptor.ActivityType, ctx.CorrelationId, workItemsBatchUid));
    }

    private async Task<Guid> EnqueuWorkItems(WorkItemSourceType sourceType, CancellationToken cancellationToken = default)
    {
        var workItems = Enumerable
                    .Range(1, new Random().Next(1, 51))
                    .Select(i => new WorkItem(sourceType, i))
                    .ToList();

        var batchUid = await _workItemSource.QueueWorkItemsAsync(sourceType, workItems, cancellationToken).ConfigureAwait(false);

        return batchUid;
    }
}
