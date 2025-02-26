using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library1;

internal sealed class Library1OperationC(string uid, ILogger logger, IActivityScheduler scheduler) : CommonActivity(uid, logger, scheduler)
{
    protected override void QueueNextActivity(ActivityContext ctx)
        => Scheduler.QueueActivity(new ActivityDescriptor("Library_1_Operation_D", typeof(Library1OperationD), ctx.CorrelationId));
}
