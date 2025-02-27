namespace Experiments.OpenTelemetry.Host;

internal interface IHostConfiguration
{
    int MaxConcurrentActivityExecution { get; }

    TimeSpan ActivityQueuePeriod { get; }

    int ActivityQueueLimit { get; }

    string PrometheusUri { get; }
}

internal interface IHostConfigurationUpdater
{
    void SetMaxConcurrentActivityExecution(int value);

    void SetActivityQueuePeriod(TimeSpan value);

    void SetActivityQueueLimit(int value);
}
