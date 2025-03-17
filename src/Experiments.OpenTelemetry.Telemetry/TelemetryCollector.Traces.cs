using System.Diagnostics;
using Experiments.OpenTelemetry.Telemetry.Resources;

namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector
{
    public Activity? StartActivity(string name, string correlationId, string? parentId = null)
    {
        try
        {
            var parentContext = new ActivityContext(
                ActivityTraceId.CreateFromString(correlationId),
                null == parentId ? default : ActivitySpanId.CreateFromString(parentId),
                ActivityTraceFlags.Recorded);

            return _activitySource.StartActivity(name, ActivityKind.Internal, parentContext);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    public Activity? SetActivityException(Activity? activity, Exception ex)
        => activity?.SetTag(Tags.ExceptionMessage, ex.Message)?.SetTag(Tags.ExceptionStackTrace, ex.StackTrace);
}
