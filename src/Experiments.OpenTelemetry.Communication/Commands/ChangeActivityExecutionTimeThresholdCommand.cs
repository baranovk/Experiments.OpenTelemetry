namespace Experiments.OpenTelemetry.Communication.Commands;

public record ChangeActivityExecutionTimeThresholdCommand(ThresholdType ThresholdType, int Milliseconds);

public enum ThresholdType { Min, Max }
