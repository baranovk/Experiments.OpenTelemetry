using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Library1;

public sealed class Library1Activity(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    Guid workItemBatchUid,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector)
    : WorkItemsProcessor(uid, logger, scheduler, workItemBatchUid, workItemSource, telemetryCollector)
{
    protected override WorkItemSourceType WorkItemSourceType => WorkItemSourceType.Type1;

    protected override Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        Scheduler.QueueActivity(
            new ActivityDescriptor("Library_1_Operation_A", typeof(Library1OperationA), ctx.CorrelationId, Some(WorkItemBatchUid))
        );

        return Task.CompletedTask;
    }
}
