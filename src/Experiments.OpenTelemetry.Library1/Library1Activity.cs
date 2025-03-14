using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Library1;

public sealed class Library1Activity(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    WorkItemsBatchDescriptor workItemsBatchDescriptor,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration)
    : WorkItemsProcessor<Library1Activity>(uid, logger, scheduler, workItemsBatchDescriptor, workItemSource, telemetryCollector, configuration)
{
    protected override Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default) => Task.CompletedTask;

    protected override async Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default)
        => await QueueNextActivity<Library1OperationA>("Lib1:A", ctx, Some(WorkItemsBatchDescriptor), cancellationToken).ConfigureAwait(false);
}
