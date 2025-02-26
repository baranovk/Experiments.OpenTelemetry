using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library1;

internal sealed class Library1OperationD(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    Guid workItemBatchUid,
    IWorkItemSource workItemSource)
    : WorkItemsProcessor(uid, logger, scheduler, workItemBatchUid, workItemSource)
{
    protected override WorkItemSourceType WorkItemSourceType => WorkItemSourceType.Type1;

    protected override async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await base.DoWork(ctx, cancellationToken).ConfigureAwait(false);
        await WorkItemSource.MarkProcessedAsync(WorkItemSourceType, WorkItemBatchUid, cancellationToken).ConfigureAwait(false);
    }
}
