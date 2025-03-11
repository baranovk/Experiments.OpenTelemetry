using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Library1;

public sealed class Library1OperationA(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    Guid workItemBatchUid,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration)
    : WorkItemsProcessor<Library1OperationA>(uid, logger, scheduler, workItemBatchUid, workItemSource, telemetryCollector, configuration)
{
    protected override WorkItemSourceType WorkItemSourceType => WorkItemSourceType.Type1;

    protected override async Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default)
        => await QueueNextActivity<Library1OperationB>("Lib1:B", ctx, Some(WorkItemBatchUid), cancellationToken).ConfigureAwait(false);
}
