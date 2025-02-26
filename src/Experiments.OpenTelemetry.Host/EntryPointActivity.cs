using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Library1;
using Experiments.OpenTelemetry.Library2;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Host;

internal sealed class EntryPointActivity(string uid, ILogger logger, IActivityScheduler scheduler) : CommonActivity(uid, logger, scheduler)
{
    private static readonly (string Name, Type Type)[] _activityDescriptors =
    [
        ("Library_1_EntryPoint_Activity", typeof(Library1Activity)),
        ("Library_2_EntryPoint_Activity", typeof(Library2Activity))
    ];

    protected override void QueueNextActivity(ActivityContext ctx)
    {
        var descriptor = _activityDescriptors[new Random().Next(_activityDescriptors.Length)];
        Scheduler.QueueActivity(new ActivityDescriptor(descriptor.Name, descriptor.Type, ctx.CorrelationId));
    }
}
