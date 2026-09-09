namespace PartnerIntegration.Api.Contracts;

public sealed record TransactionReceivedResponse(
    string Message,
    string TransactionReference);
