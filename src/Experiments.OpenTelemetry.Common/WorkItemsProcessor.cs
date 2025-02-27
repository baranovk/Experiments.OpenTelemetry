using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Common;

public abstract class WorkItemsProcessor(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    Guid workItemBatchUid,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector) : CommonActivity(uid, logger, scheduler, telemetryCollector)
{
    protected abstract WorkItemSourceType WorkItemSourceType { get; }

    protected Guid WorkItemBatchUid => workItemBatchUid;

    protected IWorkItemSource WorkItemSource => workItemSource;

    protected override async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        var rnd = new Random();
        var workItems = await WorkItemSource
                                .GetWorkItemsAsync(WorkItemSourceType, WorkItemBatchUid, cancellationToken)
                                .ConfigureAwait(false);

        foreach (var _ in workItems)
        {
            await Task.Delay(
                rnd.Next(Constants.ActivityWorkItemProcessingTimeMinSeconds, Constants.ActivityWorkItemProcessingTimeMaxSeconds + 1) * 1000,
                cancellationToken
            )
            .ConfigureAwait(false);
        }
    }
}
