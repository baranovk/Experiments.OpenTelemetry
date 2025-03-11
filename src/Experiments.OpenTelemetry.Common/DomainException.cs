namespace Experiments.OpenTelemetry.Common;

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
    Type1, Type2, Type3, Type4, Type5
}
