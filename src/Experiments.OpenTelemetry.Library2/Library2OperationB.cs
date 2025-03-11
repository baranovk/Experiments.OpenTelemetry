using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library2;

public sealed class Library2OperationB(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    Guid workItemBatchUid,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration)
    : WorkItemsProcessor<Library2OperationB>(uid, logger, scheduler, workItemBatchUid, workItemSource, telemetryCollector, configuration)
{
    protected override WorkItemSourceType WorkItemSourceType => WorkItemSourceType.Type2;

    protected override async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await base.DoWork(ctx, cancellationToken).ConfigureAwait(false);
        await WorkItemSource.MarkProcessedAsync(WorkItemSourceType, WorkItemBatchUid, cancellationToken).ConfigureAwait(false);
    }
}
