namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector : ITelemetryCollector, IDisposable
{
    #region Constructors

    public TelemetryCollector(TelemetryCollectorConfig config)
    {
        Build(config);
    }

    #endregion

    #region Public Methods

    public void Dispose()
    {
        _meterProvider?.Dispose();
        _meter?.Dispose();

        _activitySource.Dispose();
        _tracerProvider?.Dispose();
    }

    #endregion
}
