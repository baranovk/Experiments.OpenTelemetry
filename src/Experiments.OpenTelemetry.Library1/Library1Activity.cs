using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library1;

public sealed class Library1Activity(string uid, ILogger logger, IActivityScheduler scheduler) : CommonActivity(uid, logger, scheduler)
{
    protected override void QueueNextActivity(ActivityContext ctx)
        => Scheduler.QueueActivity(new ActivityDescriptor("Library_1_Operation_A", typeof(Library1OperationA), ctx.CorrelationId));
}
