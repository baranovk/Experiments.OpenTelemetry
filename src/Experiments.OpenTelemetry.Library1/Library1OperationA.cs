using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Library1;

public sealed class Library1OperationA(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    WorkItemsBatchDescriptor workItemsBatchDescriptor,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration)
    : WorkItemsProcessor<Library1OperationA>(uid, logger, scheduler, workItemsBatchDescriptor, workItemSource, telemetryCollector, configuration)
{
    protected override Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        using var activity = StartTracingActivity(ctx, "DoWork");
        return base.DoWork(ctx, cancellationToken);
    }

    protected override async Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default)
        => await QueueNextActivity<Library1OperationB>("Lib1:B", ctx, Some(WorkItemsBatchDescriptor), cancellationToken).ConfigureAwait(false);
}
