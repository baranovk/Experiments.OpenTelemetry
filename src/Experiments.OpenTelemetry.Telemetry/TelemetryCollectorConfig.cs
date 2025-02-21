namespace Experiments.OpenTelemetry.Telemetry;

public record TelemetryCollectorConfig(Uri OtlpExporterEndpoint, TimeSpan OtlpExporterTimeout);
