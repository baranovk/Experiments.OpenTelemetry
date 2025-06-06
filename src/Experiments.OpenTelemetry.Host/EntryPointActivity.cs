using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Domain;
using Experiments.OpenTelemetry.Library1;
using Experiments.OpenTelemetry.Library2;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Host;

internal sealed class EntryPointActivity(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration)
    : CommonActivity<EntryPointActivity>(uid, logger, scheduler, telemetryCollector, configuration)
{
    private readonly IWorkItemSource _workItemSource = workItemSource;

    private static readonly (string ActivityUid, Type ActivityType, WorkItemSourceType SourceType)[] _activityDescriptors =
    [
        ("Lib1:Entry", typeof(Library1Activity), WorkItemSourceType.Type1),
        ("Lib2:Entry", typeof(Library2Activity), WorkItemSourceType.Type2)
    ];

    protected override Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default) => Task.CompletedTask;

    protected override async Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        var (ActivityUid, ActivityType, SourceType) = _activityDescriptors[new Random().Next(_activityDescriptors.Length)];
        var workItemsBatchUid = await EnqueuWorkItems(SourceType, cancellationToken).ConfigureAwait(false);

        await QueueNextActivity(ActivityUid, ActivityType, ctx,
            Some(new WorkItemsBatchDescriptor(SourceType, workItemsBatchUid)), cancellationToken).ConfigureAwait(false);
    }

    private async Task<Guid> EnqueuWorkItems(WorkItemSourceType sourceType, CancellationToken cancellationToken = default)
    {
        var workItems = Enumerable
                    .Range(1, new Random().Next(1, 11))
                    .Select(i => new WorkItem(sourceType, i))
                    .ToList();

        var batchUid = await _workItemSource.QueueWorkItemsAsync(sourceType, workItems, cancellationToken).ConfigureAwait(false);

        return batchUid;
    }
}
