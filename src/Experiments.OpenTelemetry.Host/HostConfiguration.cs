namespace Experiments.OpenTelemetry.Host;

internal sealed class HostConfiguration : IHostConfiguration, IHostConfigurationUpdater
{
    private int _maxConcurrentActivityExecution = 5;
    private int _activityQueueLimit = 100;
    private readonly string _prometheusUri = "http://localhost:9090/api/v1/otlp/v1/metrics";
    private TimeSpan _activityQueuePeriod = TimeSpan.FromMilliseconds(5000);

    public int MaxConcurrentActivityExecution => _maxConcurrentActivityExecution;

    public TimeSpan ActivityQueuePeriod => _activityQueuePeriod;

    public int ActivityQueueLimit => _activityQueueLimit;

    public string PrometheusUri => _prometheusUri;

    public void SetMaxConcurrentActivityExecution(int value) => _maxConcurrentActivityExecution = value;

    public void SetActivityQueueLimit(int value) => _activityQueueLimit = value;

    public void SetActivityQueuePeriod(TimeSpan value) => _activityQueuePeriod = value;
}
