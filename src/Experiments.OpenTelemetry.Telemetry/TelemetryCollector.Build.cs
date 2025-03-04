using System.Diagnostics;
using System.Diagnostics.Metrics;
using Experiments.OpenTelemetry.Telemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryExporter = OpenTelemetry.Exporter;

namespace Experiments.OpenTelemetry.Telemetry;

public partial class TelemetryCollector
{
    #region Constants

    private const string ServiceName = "vio";

    #endregion

    #region Fields

    private MeterProvider? _meterProvider;
    private TracerProvider? _tracerProvider;
    private readonly Meter _meter = new("Experiments.OpenTelemetry.Meter");
    private readonly ActivitySource _activitySource = new("Experiments.OpenTelemetry.Tracer");
    private readonly Dictionary<string, Counter<long>> _counters = [];
    private readonly Dictionary<string, UpDownCounter<long>> _upDownCounters = [];
    private readonly Dictionary<string, Gauge<long>> _gauges = [];
    private readonly Dictionary<string, Histogram<long>> _histograms = [];

    #endregion

    private void Build(TelemetryCollectorConfig config)
    {
        try
        {
            PrepareMetricsInfrastructure(config);
            PrepareTracingInfrastructure(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private void PrepareMetricsInfrastructure(TelemetryCollectorConfig config)
    {
        _counters.Add(Counters.ActivityErrors, _meter.CreateCounter<long>(Counters.ActivityErrors));
        _gauges.Add(Gauges.ActivityQueueLength, _meter.CreateGauge<long>(Gauges.ActivityQueueLength));
        _upDownCounters.Add(Counters.ExecutingActivities, _meter.CreateUpDownCounter<long>(Counters.ExecutingActivities));
        _upDownCounters.Add(Counters.WorkItemsQueueLength, _meter.CreateUpDownCounter<long>(Counters.WorkItemsQueueLength));
        _histograms.Add(Histograms.ActivityExecutionTime, _meter.CreateHistogram<long>(Histograms.ActivityExecutionTime));

        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(_meter.Name)
            .SetExemplarFilter(ExemplarFilterType.TraceBased)
            .AddView(instrumentName: Histograms.ActivityExecutionTime,
                new ExplicitBucketHistogramConfiguration { Boundaries = [60, 300, 600, 900, 1200, 3600] })
            .ConfigureResource(BuildResources)
            //.AddConsoleExporter((options, readerOptions) =>
            //{
            //    readerOptions.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions() { ExportIntervalMilliseconds = 500 };
            //    options.Targets = OpenTelemetryExporter.ConsoleExporterOutputTargets.Console;
            //})
            .AddOtlpExporter(options =>
            {
                options.ExportProcessorType = ExportProcessorType.Simple;
                options.TimeoutMilliseconds = (int)config.OtlpMetricExporterTimeout.TotalMilliseconds;
                options.Endpoint = config.OtlpMetricExporterEndpoint;
                options.Protocol = OpenTelemetryExporter.OtlpExportProtocol.HttpProtobuf;
            })
            .Build();
    }

    private void PrepareTracingInfrastructure(TelemetryCollectorConfig config)
    {
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(_activitySource.Name)
            .ConfigureResource(BuildResources)
            //.AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.ExportProcessorType = ExportProcessorType.Simple;
                options.TimeoutMilliseconds = (int)config.OtlpTracesExporterTimeout.TotalMilliseconds;
                options.Endpoint = config.OtlpTracesExporterEndpoint;
                options.Protocol = OpenTelemetryExporter.OtlpExportProtocol.Grpc;
            })
            .Build();

    }

    private static void BuildResources(ResourceBuilder rb)
        => rb.Clear().AddService(ServiceName, serviceInstanceId: System.Net.Dns.GetHostEntry(string.Empty).HostName);
}
