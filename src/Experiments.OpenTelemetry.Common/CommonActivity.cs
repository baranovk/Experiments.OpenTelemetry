using Microsoft.Extensions.Logging;
using Unit = System.ValueTuple;

namespace Experiments.OpenTelemetry.Common;

public class CommonActivity(string uid, ILogger logger, IActivityScheduler scheduler) : ActivityBase(uid, logger, scheduler)
{
    private static readonly int ErrorSignal = new Random().Next(1, 10);

    protected override async Task<Unit> ExecuteInternalAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        var rnd = new Random();

        if (ErrorSignal == rnd.Next(1, 11))
        {
            throw new Exception($"{Uid} error");
        }

        await Task.Delay(
            new Random().Next(Constants.ActivityExecutionTimeMinSeconds, Constants.ActivityExecutionTimeMaxSeconds + 1) * 1000,
            cancellationToken
        ).ConfigureAwait(false);

        QueueNextActivity(ctx);
        return new();
    }

    protected virtual void QueueNextActivity(ActivityContext ctx) { }
}
