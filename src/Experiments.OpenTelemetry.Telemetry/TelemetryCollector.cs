namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector : IDisposable
{
    #region Fields

    private static readonly object _mutex = new();
    private static TelemetryCollector? _instance;

    #endregion

    #region Constructors

    private TelemetryCollector(TelemetryCollectorConfig config)
    {
        Build(config);
    }

    #endregion

    #region Public Methods

    public static TelemetryCollector GetInstance(TelemetryCollectorConfig config)
    {
        lock (_mutex)
        {
            _instance ??= new TelemetryCollector(config);
        }

        return _instance;
    }

    public void Dispose()
    {
        _meterProvider?.Dispose();
        _meter?.Dispose();
    }

    #endregion
}
