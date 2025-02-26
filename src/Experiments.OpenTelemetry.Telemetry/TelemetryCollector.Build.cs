using System.Diagnostics.Metrics;
using Experiments.OpenTelemetry.Telemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetryExporter = OpenTelemetry.Exporter;

namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector
{
    #region Fields

    private MeterProvider? _meterProvider;
    private Meter? _meter;
    private readonly Dictionary<string, Counter<long>> _counters = [];
    private readonly Dictionary<string, UpDownCounter<long>> _upDownCounters = [];
    private readonly Dictionary<string, Gauge<long>> _gauges = [];

    #endregion

    private void Build(TelemetryCollectorConfig config)
    {
        try
        {
            BuildMeterProvider(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private void BuildMeterProvider(TelemetryCollectorConfig config)
    {
        _meter = new("Experiments.OpenTelemetry.Meter");

        _counters.Add(Counters.ActivityErrors, _meter.CreateCounter<long>(Counters.ActivityErrors));
        _upDownCounters.Add(Counters.ExecutingActivities, _meter.CreateUpDownCounter<long>(Counters.ExecutingActivities));
        _gauges.Add(Gauges.ActivityQueueLength, _meter.CreateGauge<long>(Gauges.ActivityQueueLength));

        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(_meter.Name)
            .ConfigureResource(rb => rb.Clear().AddService("vio")
                                       .AddAttributes([new KeyValuePair<string, object>("host.id", System.Net.Dns.GetHostEntry("").HostName)]))
            .AddConsoleExporter((options, readerOptions) =>
            {
                readerOptions.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions() { ExportIntervalMilliseconds = 500 };
                options.Targets = OpenTelemetryExporter.ConsoleExporterOutputTargets.Console;
            })
            .AddOtlpExporter(options =>
            {
                options.ExportProcessorType = ExportProcessorType.Simple;
                options.TimeoutMilliseconds = (int)config.OtlpExporterTimeout.TotalMilliseconds;
                options.Endpoint = config.OtlpExporterEndpoint;
                options.Protocol = OpenTelemetryExporter.OtlpExportProtocol.HttpProtobuf;
            })
            .Build();
    }
}
