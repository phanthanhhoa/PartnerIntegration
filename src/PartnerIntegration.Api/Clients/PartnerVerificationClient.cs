using System.Net.Http.Json;
using PartnerIntegration.Api.Contracts;
using PartnerIntegration.Api.Exceptions;

namespace PartnerIntegration.Api.Clients;

public sealed class PartnerVerificationClient
    : IPartnerVerificationClient
{
    private const int MaxAttempts = 3;

    private readonly HttpClient _httpClient;
    private readonly ILogger<PartnerVerificationClient> _logger;

    public PartnerVerificationClient(
        HttpClient httpClient,
        ILogger<PartnerVerificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PartnerVerificationResponse> VerifyAsync(
        string partnerId,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 1;
             attempt <= MaxAttempts;
             attempt++)
        {
            try
            {
                var response =
                    await _httpClient.GetAsync(
                        $"/mock/partners/{partnerId}/verify",
                        cancellationToken);

                // TimeoutException from the mock endpoint
                // becomes HTTP 500 on the HTTP boundary.
                response.EnsureSuccessStatusCode();

                var result =
                    await response.Content
                        .ReadFromJsonAsync<PartnerVerificationResponse>(
                            cancellationToken);

                if (result is null)
                {
                    throw new HttpRequestException(
                        "Invalid partner verification response.");
                }

                return result;
            }
            catch (TaskCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                lastException = ex;

                _logger.LogWarning(
                    "Partner verification timeout. Attempt {Attempt}/{MaxAttempts}",
                    attempt,
                    MaxAttempts);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;

                _logger.LogWarning(
                    "Partner verification failed. Attempt {Attempt}/{MaxAttempts}",
                    attempt,
                    MaxAttempts);
            }

            if (attempt < MaxAttempts)
            {
                var delay =
                    TimeSpan.FromMilliseconds(
                        200 * Math.Pow(2, attempt - 1));

                await Task.Delay(
                    delay,
                    cancellationToken);
            }
        }

        throw new PartnerVerificationUnavailableException(
            "Partner verification failed after 3 attempts.",
            lastException);
    }
}