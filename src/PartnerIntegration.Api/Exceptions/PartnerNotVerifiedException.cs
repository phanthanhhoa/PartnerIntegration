namespace PartnerIntegration.Api.Exceptions;

public sealed class PartnerNotVerifiedException
    : Exception
{
    public PartnerNotVerifiedException(
        string partnerId)
        : base($"Partner '{partnerId}' could not be verified.")
    {
    }
}