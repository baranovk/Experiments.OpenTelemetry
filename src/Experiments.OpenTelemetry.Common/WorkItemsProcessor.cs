using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Common;

public abstract class WorkItemsProcessor<TActivity>(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    Guid workItemBatchUid,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration) : CommonActivity<TActivity>(uid, logger, scheduler, telemetryCollector, configuration)
{
    protected abstract WorkItemSourceType WorkItemSourceType { get; }

    protected Guid WorkItemBatchUid => workItemBatchUid;

    protected IWorkItemSource WorkItemSource => workItemSource;

    protected override async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        var workItems = await WorkItemSource
                                .GetWorkItemsAsync(WorkItemSourceType, WorkItemBatchUid, cancellationToken)
                                .ConfigureAwait(false);

        foreach (var _ in workItems)
        {
            await Task.Delay(
                new Random().Next(Configuration.ActivityWorkItemProcessingTimeMinMilliseconds,
                    Configuration.ActivityWorkItemProcessingTimeMaxMilliseconds + 1),
                cancellationToken
            )
            .ConfigureAwait(false);
        }
    }
}
