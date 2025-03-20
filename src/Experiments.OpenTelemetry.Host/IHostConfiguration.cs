using Experiments.OpenTelemetry.Communication.Commands;

namespace Experiments.OpenTelemetry.Host;

internal interface IHostConfiguration
{
    int MaxConcurrentExecutingActivities { get; }

    TimeSpan EntrypointActivityQueuePeriod { get; }

    int ActivityQueueLimit { get; }

    string PrometheusUri { get; }
}

internal interface IHostConfigurationUpdater
{
    void SetMaxConcurrentExecutingActivities(int value);

    void SetEntrypointActivityQueuePeriod(TimeSpan value);

    void SetActivityErrorRatePercent(Percentage percent);

    void SetActivityExecutionTimeMinMilliseconds(int value);

    void SetActivityExecutionTimeMaxMilliseconds(int value);

    void SetActivityWorkItemProcessingTimeMinMilliseconds(int value);

    void SetActivityWorkItemProcessingTimeMaxMilliseconds(int value);

    //void SetActivityQueueLimit(int value);
}
