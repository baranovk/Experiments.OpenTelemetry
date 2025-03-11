using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Communication;

namespace Experiments.OpenTelemetry.Host;

internal sealed class HostConfiguration : IHostConfiguration, IHostConfigurationUpdater, IActivityConfiguration
{
    #region Fields

    private readonly object _mutex = new();
    private int _maxConcurrentActivityExecution = 5;
    private readonly int _activityQueueLimit = 100;
    private readonly string _prometheusUri = "http://localhost:9090/api/v1/otlp/v1/metrics";
    private readonly string _jaegerUri = "http://localhost:4317";
    private TimeSpan _activityQueuePeriod = TimeSpan.FromMilliseconds(5000);
    private int _errorRatePercent = 20;
    private int _activityExecutionTimeMinMilliseconds = 1000;
    private int _activityExecutionTimeMaxMilliseconds = 6000;
    private int _activityWorkItemProcessingTimeMinMilliseconds = 1000;
    private int _activityWorkItemProcessingTimeMaxMilliseconds = 2000;

    #endregion

    #region IHostConfiguration

    public int MaxConcurrentExecutingActivities => _maxConcurrentActivityExecution;

    public TimeSpan EntrypointActivityQueuePeriod => _activityQueuePeriod;

    public int ActivityQueueLimit => _activityQueueLimit;

    public string PrometheusUri => _prometheusUri;

    public string JaegerUri => _jaegerUri;

    #endregion

    #region IActivityConfiguration

    public int ErrorRatePercent => _errorRatePercent;

    public int ActivityExecutionTimeMinMilliseconds => _activityExecutionTimeMinMilliseconds;

    public int ActivityExecutionTimeMaxMilliseconds => _activityExecutionTimeMaxMilliseconds;

    public int ActivityWorkItemProcessingTimeMinMilliseconds => _activityWorkItemProcessingTimeMinMilliseconds;

    public int ActivityWorkItemProcessingTimeMaxMilliseconds => _activityWorkItemProcessingTimeMaxMilliseconds;

    #endregion

    #region IHostConfigurationUpdater

    public void SetMaxConcurrentExecutingActivities(int value) { lock (_mutex) { _maxConcurrentActivityExecution = value; } }

    //public void SetActivityQueueLimit(int value) { lock (_mutex) { _activityQueueLimit = value; } }

    public void SetEntrypointActivityQueuePeriod(TimeSpan value) { lock (_mutex) { _activityQueuePeriod = value; } }

    public void SetActivityErrorRatePercent(Percentage percent)
    {
        lock (_mutex) { _errorRatePercent = percent.Value; }
    }

    public void SetActivityExecutionTimeMinMilliseconds(int value) { lock (_mutex) { _activityExecutionTimeMinMilliseconds = value; } }

    public void SetActivityExecutionTimeMaxMilliseconds(int value) { lock (_mutex) { _activityExecutionTimeMaxMilliseconds = value; } }

    public void SetActivityWorkItemProcessingTimeMinMilliseconds(int value) { lock (_mutex) { _activityWorkItemProcessingTimeMinMilliseconds = value; } }

    public void SetActivityWorkItemProcessingTimeMaxMilliseconds(int value) { lock (_mutex) { _activityWorkItemProcessingTimeMaxMilliseconds = value; } }

    #endregion
}
