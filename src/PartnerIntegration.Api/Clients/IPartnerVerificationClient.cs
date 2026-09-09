using PartnerIntegration.Api.Contracts;

namespace PartnerIntegration.Api.Clients;

public interface IPartnerVerificationClient
{
    Task<PartnerVerificationResponse> VerifyAsync(
        string partnerId,
        CancellationToken cancellationToken);
}