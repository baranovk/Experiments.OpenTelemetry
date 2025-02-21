using Experiments.OpenTelemetry.Common;

namespace Experiments.OpenTelemetry.Activities;

public class EntryPointActivity : ActivityBase
{
    protected override string ActivityUid => "EntryPointActivity";

    protected override async Task ExecuteInternalAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await Task.Delay(new Random().Next(10000, 30000), cancellationToken).ConfigureAwait(false);
    }
}
