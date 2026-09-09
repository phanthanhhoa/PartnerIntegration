using PartnerIntegration.Api.Clients;
using PartnerIntegration.Api.Contracts;
using PartnerIntegration.Api.Exceptions;
using PartnerIntegration.Api.Messaging;

namespace PartnerIntegration.Api.Services;

public sealed class PartnerTransactionService
    : IPartnerTransactionService
{
    private readonly IPartnerVerificationClient
        _partnerVerificationClient;

    private readonly IMessagePublisher
        _messagePublisher;

    public PartnerTransactionService(
        IPartnerVerificationClient partnerVerificationClient,
        IMessagePublisher messagePublisher)
    {
        _partnerVerificationClient =
            partnerVerificationClient;

        _messagePublisher =
            messagePublisher;
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

        var message =
            new PartnerTransactionMessage(
                PartnerId: request.PartnerId!,
                TransactionReference:
                    request.TransactionReference!,
                Amount: request.Amount,
                Currency:
                    request.Currency!.ToUpperInvariant(),
                Timestamp:
                    request.Timestamp!.Value);

        await _messagePublisher.PublishAsync(
            message,
            cancellationToken);

        return new TransactionReceivedResponse(
            "Transaction accepted",
            request.TransactionReference!);
    }
}