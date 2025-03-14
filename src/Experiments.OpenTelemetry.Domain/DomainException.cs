namespace Experiments.OpenTelemetry.Domain;

public class DomainException(DomainErrorType errorType, string? message, Exception? innerException) : Exception(message, innerException)
{
    public DomainException(DomainErrorType errorType) : this(errorType, null, null)
    {
    }

    public DomainException(DomainErrorType errorType, string message) : this(errorType, message, null)
    {
    }

    public DomainErrorType ErrorType { get; private set; } = errorType;
}

public enum DomainErrorType
{
    ErrorType1, ErrorType2, ErrorType3, ErrorType4, ErrorType5
}
