namespace PartnerIntegration.Api.Messaging;

public sealed record PartnerTransactionMessage(
    string PartnerId,
    string TransactionReference,
    decimal Amount,
    string Currency,
    DateTimeOffset Timestamp);