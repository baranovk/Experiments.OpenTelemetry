using Experiments.OpenTelemetry.Activities;
using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Telemetry;

namespace Experiments.OpenTelemetry.Host;

internal static class Program
{
    #region Constants

    private const int ActivityQueueLimit = 100;
    private const int ActivityQueuePeriod = 15000;

    #endregion

    #region Fields

    private static ActivityScheduler? _entrypointScheduler;
    private static ActivityScheduler? _activityScheduler;

    #endregion

    static void Main()
    {
        try
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) => cts.Cancel();

            Init(cts.Token);
            Run(cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            ShutDown();
        }
    }

    private static void Init(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _entrypointScheduler = new ActivityScheduler(ActivityQueueLimit, cancellationToken);
        _activityScheduler = new ActivityScheduler(ActivityQueueLimit, cancellationToken);

        var telemetryCollectorConfig = new TelemetryCollectorConfig(new Uri("http"), TimeSpan.FromMilliseconds(3000));

        _entrypointScheduler.Subscribe(new ActivityExecutor(_activityScheduler, telemetryCollectorConfig, cancellationToken));
        _activityScheduler.Subscribe(new ActivityExecutor(_activityScheduler, telemetryCollectorConfig, cancellationToken));
    }

    private static void Run(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _entrypointScheduler?.QueueActivity(new ActivityDescriptor(typeof(EntryPointActivity)));

                Thread.Sleep(ActivityQueuePeriod);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Terminating... See ya!");
        }
    }

    private static void ShutDown()
    {
        _entrypointScheduler?.Dispose();
        _activityScheduler?.Dispose();
        Console.WriteLine("ShutDown OK");
    }
}
