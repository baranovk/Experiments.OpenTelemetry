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
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration)
    : WorkItemsProcessor<Library2Activity>(uid, logger, scheduler, workItemBatchUid, workItemSource, telemetryCollector, configuration)
{
    protected override WorkItemSourceType WorkItemSourceType => WorkItemSourceType.Type2;

    protected override Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default) => Task.CompletedTask;

    protected override async Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default)
        => await QueueNextActivity<Library2OperationA>("Lib2:A", ctx, Some(WorkItemBatchUid), cancellationToken).ConfigureAwait(false);
}
