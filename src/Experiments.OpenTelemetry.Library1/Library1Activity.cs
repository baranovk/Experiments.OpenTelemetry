using Experiments.OpenTelemetry.Common;

namespace Experiments.OpenTelemetry.Library1;

internal sealed class Library1Activity : ActivityBase
{
    protected override string ActivityUid => "Library1Activity";

    protected override async Task ExecuteInternalAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await Task.Delay(new Random().Next(10000, 30000), cancellationToken).ConfigureAwait(false);
    }
}
