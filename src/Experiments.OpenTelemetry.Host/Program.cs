using Experiments.OpenTelemetry.Activities;
using Experiments.OpenTelemetry.Telemetry;

namespace Experiments.OpenTelemetry.Host;

internal static class Program
{
    #region Constants

    private const int ActivityQueueLimit = 100;
    private const int ActivityQueuePeriod = 15000;

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

        ActivityScheduler.Init(ActivityQueueLimit, cancellationToken);

        var telemetryCollectorConfig = new TelemetryCollectorConfig(new Uri("http"), TimeSpan.FromMilliseconds(3000));
        var activityExecutor = new ActivityExecutor(telemetryCollectorConfig, cancellationToken);
        ActivityScheduler.Instance.Subscribe(activityExecutor);
    }

    private static void Run(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ActivityScheduler.Instance.QueueActivity(new ActivityDescriptor(typeof(EntryPointActivity)));

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
        ActivityScheduler.Instance.Dispose();
        Console.WriteLine("ShutDown OK");
    }
}
