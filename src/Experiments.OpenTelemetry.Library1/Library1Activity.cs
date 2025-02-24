using Experiments.OpenTelemetry.Common;
using Unit = System.ValueTuple;

namespace Experiments.OpenTelemetry.Library1;

internal sealed class Library1Activity : ActivityBase
{
    public Library1Activity(IActivityScheduler scheduler) : base(scheduler)
    {
    }

    public override string Uid => "Library1Activity";

    protected override async Task<Unit> ExecuteInternalAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await Task.Delay(new Random().Next(10000, 30000), cancellationToken).ConfigureAwait(false);
        return new();
    }
}
