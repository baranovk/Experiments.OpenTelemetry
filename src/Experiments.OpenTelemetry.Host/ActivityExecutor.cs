using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;

namespace Experiments.OpenTelemetry.Host;

internal sealed class ActivityExecutor(TelemetryCollectorConfig telemetryCollectorConfig, CancellationToken cancellationToken = default) : IObserver<ActivityDescriptor>
{
    private readonly TelemetryCollectorConfig _telemetryCollectorConfig = telemetryCollectorConfig;
    private readonly CancellationToken _cancellationToken = cancellationToken;

    public void OnNext(ActivityDescriptor value)
    {
        var ctx = new ActivityContext(_telemetryCollectorConfig);
        var activity = Activator.CreateInstance(value.ActivityType, ctx);

        Task.Factory.StartNew(
            async () => await (activity as IProcessFlowJobActivity)!.ExecuteAsync(ctx, _cancellationToken).ConfigureAwait(false),
            _cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        )
        .ContinueWith(t => Console.WriteLine(t.Exception!.ToString()),
            default, TaskContinuationOptions.NotOnFaulted, TaskScheduler.Default);
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }
}
