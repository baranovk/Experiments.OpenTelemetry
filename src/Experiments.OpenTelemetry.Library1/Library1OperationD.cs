using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library1;

internal sealed class Library1OperationD(string uid, ILogger logger, IActivityScheduler scheduler) : CommonActivity(uid, logger, scheduler)
{
}
