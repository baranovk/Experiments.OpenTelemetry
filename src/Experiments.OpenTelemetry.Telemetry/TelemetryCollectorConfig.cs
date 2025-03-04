namespace Experiments.OpenTelemetry.Telemetry;

public record TelemetryCollectorConfig(
    Uri OtlpMetricExporterEndpoint,
    TimeSpan OtlpMetricExporterTimeout,
    Uri OtlpTracesExporterEndpoint,
    TimeSpan OtlpTracesExporterTimeout
);
