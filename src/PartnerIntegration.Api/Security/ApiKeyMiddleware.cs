using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegration.Api.Security;

public sealed class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-API-Key";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(
        RequestDelegate next,
        IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only secure the partner transaction endpoint.
        if (!context.Request.Path.StartsWithSegments(
                "/api/v1/partner/transactions"))
        {
            await _next(context);
            return;
        }

        var expectedApiKey = _configuration["Security:ApiKey"];

        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            throw new InvalidOperationException(
                "API key is not configured.");
        }

        if (!context.Request.Headers.TryGetValue(
                ApiKeyHeaderName,
                out var providedApiKey))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        if (!string.Equals(
                providedApiKey.ToString(),
                expectedApiKey,
                StringComparison.Ordinal))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        await _next(context);
    }

    private static async Task WriteUnauthorizedAsync(
        HttpContext context)
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,

                Title = "Unauthorized",

                Detail = $"A valid {ApiKeyHeaderName} header is required."
            });
    }
}