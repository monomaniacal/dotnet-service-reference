using ConfigService.Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ConfigService.Api.Tests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_ReturnsTrueAndLogsError()
    {
        var problemDetailsService = Substitute.For<IProblemDetailsService>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(problemDetailsService, hostEnvironment, logger);
        var httpContext = new DefaultHttpContext();
        var exception = new InvalidOperationException("boom");

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        await problemDetailsService.Received(1).TryWriteAsync(Arg.Any<ProblemDetailsContext>());
    }
}
