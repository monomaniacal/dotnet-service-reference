namespace ConfigService.Api.Infrastructure;

public sealed class ReferencedResourceNotFoundException : Exception
{
    public ReferencedResourceNotFoundException(string message)
        : base(message)
    {
    }

    public ReferencedResourceNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
