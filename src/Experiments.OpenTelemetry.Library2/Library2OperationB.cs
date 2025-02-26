using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library2;

internal sealed class Library2OperationB(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    Guid workItemBatchUid,
    IWorkItemSource workItemSource)
    : WorkItemsProcessor(uid, logger, scheduler, workItemBatchUid, workItemSource)
{
    protected override WorkItemSourceType WorkItemSourceType => WorkItemSourceType.Type2;

    protected override async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await base.DoWork(ctx, cancellationToken).ConfigureAwait(false);
        //var workItems = WorkItemSource.GetWorkItemsAsync(WorkItemSourceType, WorkItemBatchUid, cancellationToken);
        await WorkItemSource.MarkProcessedAsync(WorkItemSourceType, WorkItemBatchUid, cancellationToken).ConfigureAwait(false);
    }
}
