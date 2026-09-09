using PartnerIntegration.Api.Clients;
using PartnerIntegration.Api.Contracts;
using PartnerIntegration.Api.Exceptions;
using PartnerIntegration.Api.Messaging;
using PartnerIntegration.Api.Services;
using Xunit;

namespace PartnerIntegration.Tests.Services;

public sealed class PartnerTransactionServiceTests
{
    [Fact]
    public async Task ProcessAsync_Should_Publish_When_PartnerIsValid()
    {
        var partnerClient =
            new FakePartnerVerificationClient(true);

        var publisher =
            new FakeMessagePublisher();

        var service =
            new PartnerTransactionService(
                partnerClient,
                publisher);

        var result =
            await service.ProcessAsync(
                CreateValidRequest(),
                CancellationToken.None);

        Assert.Equal(
            "Transaction accepted",
            result.Message);

        Assert.Equal(
            1,
            publisher.PublishCount);

        Assert.NotNull(
            publisher.LastMessage);

        Assert.Equal(
            "TXN-99823",
            publisher.LastMessage!
                .TransactionReference);
    }

        [Fact]
    public async Task ProcessAsync_Should_NotPublish_When_PartnerIsInvalid()
    {
        var partnerClient =
            new FakePartnerVerificationClient(false);

        var publisher =
            new FakeMessagePublisher();

        var service =
            new PartnerTransactionService(
                partnerClient,
                publisher);

        await Assert.ThrowsAsync<
            PartnerNotVerifiedException>(
                () => service.ProcessAsync(
                    CreateValidRequest(),
                    CancellationToken.None));

        Assert.Equal(
            0,
            publisher.PublishCount);
    }

    private sealed class FakePartnerVerificationClient
        : IPartnerVerificationClient
    {
        private readonly bool _isValid;

        public FakePartnerVerificationClient(
            bool isValid)
        {
            _isValid = isValid;
        }

        public Task<PartnerVerificationResponse>
            VerifyAsync(
                string partnerId,
                CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new PartnerVerificationResponse(
                    partnerId,
                    _isValid));
        }
    }

    private sealed class FakeMessagePublisher
        : IMessagePublisher
    {
        public int PublishCount { get; private set; }

        public PartnerTransactionMessage?
            LastMessage { get; private set; }

        public Task PublishAsync(
            PartnerTransactionMessage message,
            CancellationToken cancellationToken)
        {
            PublishCount++;
            LastMessage = message;

            return Task.CompletedTask;
        }
    }

    private static CreatePartnerTransactionRequest
        CreateValidRequest()
    {
        return new CreatePartnerTransactionRequest(
            "P-1001",
            "TXN-99823",
            250m,
            "USD",
            DateTimeOffset.Parse(
                "2024-05-10T14:30:00Z"));
    }
}