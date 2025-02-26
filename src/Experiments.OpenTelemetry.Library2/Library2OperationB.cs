using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Library2;

internal sealed class Library2OperationB(string uid, ILogger logger, IActivityScheduler scheduler) : CommonActivity(uid, logger, scheduler)
{
}
