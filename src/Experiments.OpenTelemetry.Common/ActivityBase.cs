using System.Diagnostics;
using Experiments.OpenTelemetry.Telemetry;

namespace Experiments.OpenTelemetry.Common;

public abstract class ActivityBase : IProcessFlowJobActivity
{
    public async Task ExecuteAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        var collector = TelemetryCollector.GetInstance(ctx.TelemetryCollectorConfig);

        collector.IncrementExecutingActivityCounter(ActivityUid);
        var sw = new Stopwatch();
        sw.Start();
        await ExecuteInternalAsync(ctx, cancellationToken).ConfigureAwait(false);
        sw.Stop();
        collector.DecrementExecutingActivityCounter(ActivityUid);
    }

    protected abstract string ActivityUid { get; }

    protected abstract Task ExecuteInternalAsync(ActivityContext ctx, CancellationToken cancellationToken = default);
}
