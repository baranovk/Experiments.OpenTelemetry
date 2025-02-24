using Experiments.OpenTelemetry.Common;
using Unit = System.ValueTuple;

namespace Experiments.OpenTelemetry.Activities;

public class EntryPointActivity : ActivityBase
{
    public EntryPointActivity(IActivityScheduler scheduler) : base(scheduler)
    {
    }

    public override string Uid => "EntryPointActivity";

    protected override async Task<Unit> ExecuteInternalAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await Task.Delay(new Random().Next(10000, 30000), cancellationToken).ConfigureAwait(false);
        return new();
    }
}
