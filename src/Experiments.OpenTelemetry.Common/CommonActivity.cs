using Experiments.OpenTelemetry.Telemetry;
using Microsoft.Extensions.Logging;
using Unit = System.ValueTuple;

namespace Experiments.OpenTelemetry.Common;

public class CommonActivity(
    string uid,
    ILogger logger,
    IActivityScheduler scheduler,
    ITelemetryCollector telemetryCollector) : ActivityBase(uid, logger, scheduler, telemetryCollector)
{
    private static readonly int ErrorSignal = new Random().Next(0, 5);

    protected override async Task<Unit> ExecuteInternalAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        //await Task.Yield();
        //throw new Exception($"{Uid} error");
        var rnd = new Random();

        if (ErrorSignal == rnd.Next(0, 5))
        {
            throw new Exception($"{Uid} error");
        }

        await DoWork(ctx, cancellationToken).ConfigureAwait(false);
        await QueueNextActivity(ctx, cancellationToken).ConfigureAwait(false);

        return new();
    }

    protected virtual async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await Task.Delay(
            new Random().Next(Constants.ActivityExecutionTimeMinSeconds, Constants.ActivityExecutionTimeMaxSeconds + 1) * 1000,
            cancellationToken
        )
        .ConfigureAwait(false);
    }

    protected virtual Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
