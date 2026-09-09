namespace PartnerIntegration.Api.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(
        PartnerTransactionMessage message,
        CancellationToken cancellationToken);
}