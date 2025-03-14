using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library2;

public sealed class Library2OperationB(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    WorkItemsBatchDescriptor workItemsBatchDescriptor,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration)
    : WorkItemsProcessor<Library2OperationB>(uid, logger, scheduler, workItemsBatchDescriptor, workItemSource, telemetryCollector, configuration)
{
    protected override async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await base.DoWork(ctx, cancellationToken).ConfigureAwait(false);
        await WorkItemSource.MarkProcessedAsync(WorkItemsBatchDescriptor.SourceType,
            WorkItemsBatchDescriptor.Uid, cancellationToken).ConfigureAwait(false);
    }
}
