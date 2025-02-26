using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library2;

internal sealed class Library2OperationA(string uid, ILogger logger, IActivityScheduler scheduler) : CommonActivity(uid, logger, scheduler)
{
    protected override void QueueNextActivity(ActivityContext ctx)
        => Scheduler.QueueActivity(new ActivityDescriptor("Library_2_Operation_B", typeof(Library2OperationB), ctx.CorrelationId));
}
