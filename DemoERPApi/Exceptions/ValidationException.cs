namespace DemoERPApi.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }


    public ValidationException(
        IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }


    public ValidationException(
        string message)
        : base(message)
    {
        Errors =
            new Dictionary<string, string[]>();
    }


    public ValidationException(
        string field,
        string error)
        : base("Validation failed.")
    {
        Errors =
            new Dictionary<string, string[]>
            {
                {
                    field,
                    new[]
                    {
                        error
                    }
                }
            };
    }
}