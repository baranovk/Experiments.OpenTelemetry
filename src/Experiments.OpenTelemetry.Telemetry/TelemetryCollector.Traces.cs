using System.Diagnostics;

namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector
{
    public Activity? StartActivity(string name, string correlationId)
    {
        try
        {
            var parentContext = new ActivityContext(
                ActivityTraceId.CreateFromString(correlationId),
                ActivitySpanId.CreateRandom(),
                ActivityTraceFlags.Recorded);

            return _activitySource.StartActivity(name, ActivityKind.Internal, parentContext);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}
