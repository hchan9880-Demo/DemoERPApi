namespace DemoERPApi.Exceptions;


/// Exception for unauthorized access scenarios.

public class UnauthorizedException : Exception
{
    public string? ErrorCode { get; }

    public UnauthorizedException(string message) : base(message) { }

    public UnauthorizedException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException) { }
}