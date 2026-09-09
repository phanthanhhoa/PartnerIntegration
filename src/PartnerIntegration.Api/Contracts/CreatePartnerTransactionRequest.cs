namespace PartnerIntegration.Api.Contracts;

public sealed record CreatePartnerTransactionRequest(
    string? PartnerId,
    string? TransactionReference,
    decimal Amount,
    string? Currency,
    DateTimeOffset? Timestamp);
