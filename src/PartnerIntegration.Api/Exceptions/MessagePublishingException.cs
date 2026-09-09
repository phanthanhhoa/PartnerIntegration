namespace PartnerIntegration.Api.Exceptions;

public sealed class MessagePublishingException
    : Exception
{
    public MessagePublishingException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}