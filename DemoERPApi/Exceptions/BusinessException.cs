namespace DemoERPApi.Exceptions;


/// Exception for business rule violations with error code support.

public class BusinessException : Exception
{
    public string Code { get; }
    public object? Details { get; }

    public BusinessException(string code, string message, object? details = null)
        : base(message)
    {
        Code = code;
        Details = details;
    }
}