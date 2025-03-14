namespace Experiments.OpenTelemetry.Domain;

public record WorkItem(WorkItemSourceType SourceType, int Value);

public enum WorkItemSourceType
{
    Type1,
    Type2
}
