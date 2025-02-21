using System.Diagnostics.Metrics;
using Experiments.OpenTelemetry.Telemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetryExporter = OpenTelemetry.Exporter;

namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector
{
    #region Fields

    private MeterProvider? _meterProvider;
    private Meter? _meter;

    #endregion

    private void Build(TelemetryCollectorConfig config)
    {
        BuildMeterProvider(config);
    }

    private void BuildMeterProvider(TelemetryCollectorConfig config)
    {
        _meter = new("Experiments.OpenTelemetry.Meter");
        //_counters.Add(CounterExecuteActivity, _meter.CreateCounter<long>(CounterExecuteActivity));
        _upDownCounters.Add(Counters.ExecutingActivities, _meter.CreateUpDownCounter<long>(Counters.ExecutingActivities));

        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(_meter.Name)
            .AddConsoleExporter((options, readerOptions) =>
            {
                readerOptions.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions() { ExportIntervalMilliseconds = 500 };
                options.Targets = OpenTelemetryExporter.ConsoleExporterOutputTargets.Console;
            })
            .AddOtlpExporter(options =>
            {
                options.ExportProcessorType = ExportProcessorType.Simple;
                options.TimeoutMilliseconds = config.OtlpExporterTimeout.Milliseconds;
                options.Endpoint = config.OtlpExporterEndpoint;
                options.Protocol = OpenTelemetryExporter.OtlpExportProtocol.HttpProtobuf;
            })
            .Build();
    }
}
