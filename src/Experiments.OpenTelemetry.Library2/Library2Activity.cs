using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library2;

public sealed class Library2Activity(string uid, ILogger logger, IActivityScheduler scheduler) : CommonActivity(uid, logger, scheduler)
{
    protected override void QueueNextActivity(ActivityContext ctx)
        => Scheduler.QueueActivity(new ActivityDescriptor("Library_2_Operation_A", typeof(Library2OperationA), ctx.CorrelationId));
}
