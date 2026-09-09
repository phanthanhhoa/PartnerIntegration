using PartnerIntegration.Api.Contracts;

namespace PartnerIntegration.Api.Services;

public interface IPartnerTransactionService
{
    Task<TransactionReceivedResponse> ProcessAsync(
        CreatePartnerTransactionRequest request,
        CancellationToken cancellationToken);
}