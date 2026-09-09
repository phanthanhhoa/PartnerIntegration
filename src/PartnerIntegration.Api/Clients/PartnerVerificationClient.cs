using System.Net.Http.Json;
using PartnerIntegration.Api.Contracts;
using PartnerIntegration.Api.Exceptions;

namespace PartnerIntegration.Api.Clients;

public sealed class PartnerVerificationClient
    : IPartnerVerificationClient
{
    private readonly HttpClient _httpClient;

    public PartnerVerificationClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PartnerVerificationResponse> VerifyAsync(
        string partnerId,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response =
                await _httpClient.GetAsync(
                    $"/api/v1/mock/partners/{partnerId}/verify",
                    cancellationToken);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content
                    .ReadFromJsonAsync<PartnerVerificationResponse>(
                        cancellationToken);

            return result
                ?? throw new PartnerVerificationUnavailableException(
                    "Partner verification returned an empty response.");
        }
        catch (HttpRequestException ex)
        {
            throw new PartnerVerificationUnavailableException(
                "Partner verification service is unavailable.",
                ex);
        }
    }
}