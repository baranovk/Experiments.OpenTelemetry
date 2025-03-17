using Experiments.OpenTelemetry.Domain;
using Experiments.OpenTelemetry.Telemetry;
using Functional;
using Microsoft.Extensions.Logging;
using static Functional.F;
using Unit = System.ValueTuple;

namespace Experiments.OpenTelemetry.Common;

public class CommonActivity<TActivity> : ActivityBase
{
    private readonly int _errorRate;

    public CommonActivity(string uid,
        ILogger logger,
        IActivityScheduler scheduler,
        ITelemetryCollector telemetryCollector,
        IActivityConfiguration configuration) : base(uid, logger, scheduler, telemetryCollector, configuration)
    {
        _errorRate = ValidateErrorRate(configuration.ErrorRatePercent)
            .Match(
                errors => throw new InvalidOperationException(errors.First().Message),
                rate => rate
            );
    }

    protected override async Task<Unit> ExecuteInternalAsync(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        if (0 == (IncrementExecutionCount(typeof(TActivity)) % _errorRate))
        {
            Console.WriteLine("");
        }

        await Task.Yield();
        throw GenerateException();
        //if (0 == (IncrementExecutionCount(typeof(TActivity)) % _errorRate))
        //{
        //    throw GenerateException();
        //}

        //using (var activity = StartTracingActivity(ctx, "DoWork"))
        //{
        //    await DoWork(ctx, cancellationToken).ConfigureAwait(false);
        //}

        //await QueueNextActivity(ctx, cancellationToken).ConfigureAwait(false);

        //return new();
    }

    protected virtual async Task DoWork(ActivityContext ctx, CancellationToken cancellationToken = default)
    {
        await Task.Delay(
            new Random().Next(Configuration.ActivityExecutionTimeMinMilliseconds, Configuration.ActivityExecutionTimeMaxMilliseconds + 1),
            cancellationToken
        )
        .ConfigureAwait(false);
    }

    protected virtual Task QueueNextActivity(ActivityContext ctx, CancellationToken cancellationToken = default) => Task.CompletedTask;

    private Validation<int> ValidateErrorRate(int errorRatePercent) => 0 <= errorRatePercent && errorRatePercent <= 100
            ? Valid(100 / Configuration.ErrorRatePercent) : Invalid<int>("Invalid error rate percent");

    private DomainException GenerateException()
    {
        var errorTypes = Enum.GetValues<DomainErrorType>();
        return new DomainException(errorTypes[new Random().Next(0, errorTypes.Length)], $"{Uid} error");
    }
}
