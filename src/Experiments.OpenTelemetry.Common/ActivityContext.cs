using Experiments.OpenTelemetry.Telemetry;

namespace Experiments.OpenTelemetry.Common;

public record ActivityContext(TelemetryCollectorConfig TelemetryCollectorConfig);
