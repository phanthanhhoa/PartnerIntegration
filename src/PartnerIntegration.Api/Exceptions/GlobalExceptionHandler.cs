using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegration.Api.Exceptions;

public sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) =
            exception switch
            {
                PartnerNotVerifiedException =>
                    (
                        StatusCodes.Status400BadRequest,
                        "Partner verification failed"
                    ),

                PartnerVerificationUnavailableException =>
                    (
                        StatusCodes.Status503ServiceUnavailable,
                        "Partner verification unavailable"
                    ),

                MessagePublishingException =>
                    (
                        StatusCodes.Status503ServiceUnavailable,
                        "Message broker unavailable"
                    ),

                _ =>
                    (
                        StatusCodes.Status500InternalServerError,
                        "Unexpected server error"
                    )
            };

        _logger.LogError(
            exception,
            "Request failed with status {StatusCode}. TraceId: {TraceId}",
            statusCode,
            httpContext.TraceIdentifier);

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,

                Detail =
                    statusCode ==
                    StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred."
                        : exception.Message,

                Instance =
                    httpContext.Request.Path
            };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode =
            statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}