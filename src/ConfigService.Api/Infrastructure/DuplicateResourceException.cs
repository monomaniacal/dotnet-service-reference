namespace ConfigService.Api.Infrastructure;

public sealed class DuplicateResourceException : Exception
{
    public DuplicateResourceException(string message)
        : base(message)
    {
    }

    public DuplicateResourceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
