using PartnerIntegration.Api.Contracts;

namespace PartnerIntegration.Api.Validation;

public sealed class TransactionRequestValidator
{
    private static readonly HashSet<string> ValidCurrencies =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "USD",
            "EUR",
            "VND"
        };

    public Dictionary<string, string[]> Validate(CreatePartnerTransactionRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.PartnerId))
            errors["partnerId"] = ["PartnerId is required."];

        if (string.IsNullOrWhiteSpace(request.TransactionReference))
            errors["transactionReference"] = ["TransactionReference is required."];

        if (request.Amount <= 0)
            errors["amount"] = ["Amount must be greater than 0."];

        if (string.IsNullOrWhiteSpace(request.Currency))
            errors["currency"] = ["Currency is required."];
        else if (!ValidCurrencies.Contains(request.Currency))
            errors["currency"] = ["Currency is invalid."];

        if (request.Timestamp is null)
            errors["timestamp"] = ["Timestamp is required."];

        return errors;
    }
}
