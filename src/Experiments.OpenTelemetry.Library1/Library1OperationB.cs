using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Library1;

public sealed class Library1OperationB(string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    WorkItemsBatchDescriptor workItemsBatchDescriptor,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration)
    : WorkItemsProcessor<Library1OperationB>(uid, logger, scheduler, workItemsBatchDescriptor, workItemSource, telemetryCollector, configuration)
{
    protected override async Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default)
        => await QueueNextActivity<Library1OperationC>("Lib1:C", ctx, Some(WorkItemsBatchDescriptor), cancellationToken).ConfigureAwait(false);
}
