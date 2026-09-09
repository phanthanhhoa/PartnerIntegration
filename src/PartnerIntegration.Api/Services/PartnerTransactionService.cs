using PartnerIntegration.Api.Clients;
using PartnerIntegration.Api.Contracts;
using PartnerIntegration.Api.Exceptions;

namespace PartnerIntegration.Api.Services;

public sealed class PartnerTransactionService
    : IPartnerTransactionService
{
    private readonly IPartnerVerificationClient
        _partnerVerificationClient;

    public PartnerTransactionService(
        IPartnerVerificationClient partnerVerificationClient)
    {
        _partnerVerificationClient =
            partnerVerificationClient;
    }

    public async Task<TransactionReceivedResponse> ProcessAsync(
        CreatePartnerTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var partner =
            await _partnerVerificationClient.VerifyAsync(
                request.PartnerId!,
                cancellationToken);

        if (!partner.IsValid)
        {
            throw new PartnerNotVerifiedException(
                request.PartnerId!);
        }

        return new TransactionReceivedResponse(
            "Transaction received",
            request.TransactionReference!);
    }
}