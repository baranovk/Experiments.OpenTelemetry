namespace Experiments.OpenTelemetry.Communication;

public record ChangeActivityExecutionTimeThresholdCommand(ThresholdType ThresholdType, int Milliseconds);

public enum ThresholdType { Min, Max }
