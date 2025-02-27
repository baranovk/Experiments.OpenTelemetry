using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Library2;

public sealed class Library2Activity(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    Guid workItemBatchUid,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector)
    : WorkItemsProcessor(uid, logger, scheduler, workItemBatchUid, workItemSource, telemetryCollector)
{
    protected override WorkItemSourceType WorkItemSourceType => WorkItemSourceType.Type2;

    protected override Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        Scheduler.QueueActivity(
            new ActivityDescriptor("Library_2_Operation_A", typeof(Library2OperationA), ctx.CorrelationId, Some(WorkItemBatchUid))
        );

        return Task.CompletedTask;
    }
}
