using Experiments.OpenTelemetry.Domain;
using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Common;

public abstract class WorkItemsProcessor<TActivity>(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    WorkItemsBatchDescriptor workItemsBatchDescriptor,
    IWorkItemSource workItemSource,
    ITelemetryCollector telemetryCollector,
    IActivityConfiguration configuration) : CommonActivity<TActivity>(uid, logger, scheduler, telemetryCollector, configuration)
{
    protected WorkItemsBatchDescriptor WorkItemsBatchDescriptor { get; private set; } = workItemsBatchDescriptor;

    protected IWorkItemSource WorkItemSource => workItemSource;

    protected override async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        var workItems = await WorkItemSource
                                .GetWorkItemsAsync(WorkItemsBatchDescriptor.SourceType, WorkItemsBatchDescriptor.Uid, cancellationToken)
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

    protected override void IncrementExecutingActivityCounter()
        => TelemetryCollector.IncrementExecutingActivityCounter(Uid, WorkItemsBatchDescriptor.SourceType);

    protected override void DecrementExecutingActivityCounter()
        => TelemetryCollector.DecrementExecutingActivityCounter(Uid, WorkItemsBatchDescriptor.SourceType);

    protected override void IncrementActivityErrorCounter(string errorType)
        => TelemetryCollector.IncrementActivityErrorCounter(Uid, errorType, WorkItemsBatchDescriptor.SourceType);

    protected override void RecordActivityExecutionTime(TimeSpan executionTime)
        => TelemetryCollector.RecordActivityExecutionTime(Uid, executionTime, WorkItemsBatchDescriptor.SourceType);
}
