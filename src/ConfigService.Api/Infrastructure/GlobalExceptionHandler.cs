using Microsoft.AspNetCore.Diagnostics;

namespace ConfigService.Api.Infrastructure;

public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment hostEnvironment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            DuplicateResourceException duplicate =>
                (StatusCodes.Status409Conflict, "Conflict", duplicate.Message),
            ReferencedResourceNotFoundException referenced =>
                (StatusCodes.Status404NotFound, "Not Found", referenced.Message),
            _ =>
                (StatusCodes.Status500InternalServerError, "An unexpected error occurred.",
                    hostEnvironment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred while processing the request."),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            LogUnhandledException(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
            },
        });

        return true;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception processing {Method} {Path}")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string method, PathString path);
}
