using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library1;

public sealed class Library1OperationD(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    WorkItemsBatchDescriptor workItemsBatchDescriptor,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration)
    : WorkItemsProcessor<Library1OperationD>(uid, logger, scheduler, workItemsBatchDescriptor, workItemSource, telemetryCollector, configuration)
{
    protected override async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await base.DoWork(ctx, cancellationToken).ConfigureAwait(false);
        await WorkItemSource.MarkProcessedAsync(WorkItemsBatchDescriptor.SourceType,
            WorkItemsBatchDescriptor.Uid, cancellationToken).ConfigureAwait(false);
    }
}
