using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library1;

internal sealed class Library1OperationB(string uid, ILogger logger, IActivityScheduler scheduler) : CommonActivity(uid, logger, scheduler)
{
    protected override void QueueNextActivity(ActivityContext ctx)
        => Scheduler.QueueActivity(new ActivityDescriptor("Library_1_Operation_C", typeof(Library1OperationC), ctx.CorrelationId));
}
