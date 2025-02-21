using System.Threading;

namespace Experiments.OpenTelemetry.Common;

public interface IProcessFlowJobActivity
{
    public Task ExecuteAsync(ActivityContext ctx, CancellationToken cancellationToken = default);
}
