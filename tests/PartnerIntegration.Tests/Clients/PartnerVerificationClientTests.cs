using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using PartnerIntegration.Api.Clients;
using PartnerIntegration.Api.Contracts;
using PartnerIntegration.Api.Exceptions;
using Xunit;

namespace PartnerIntegration.Tests.Clients;

public sealed class PartnerVerificationClientTests
{
    [Fact]
    public async Task VerifyAsync_Should_Retry_And_Succeed_When_TransientFailuresOccur()
    {
        // Arrange
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            CreateSuccessResponse());

        var services = new ServiceCollection();

        services.AddLogging();

        services
            .AddHttpClient<
                IPartnerVerificationClient,
                PartnerVerificationClient>(client =>
            {
                client.BaseAddress =
                    new Uri("http://localhost");
            })
            .ConfigurePrimaryHttpMessageHandler(
                () => handler)
            .AddStandardResilienceHandler(options =>
            {
                // Total: 1 initial request + 2 retries = 3 attempts
                options.Retry.MaxRetryAttempts = 2;

                options.Retry.Delay =
                    TimeSpan.Zero;

                options.Retry.UseJitter =
                    false;

                options.AttemptTimeout.Timeout =
                    TimeSpan.FromSeconds(2);

                options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(5);
            });

        await using var provider =
            services.BuildServiceProvider();

        var client =
            provider.GetRequiredService<
                IPartnerVerificationClient>();

        // Act
        var result =
            await client.VerifyAsync(
                "P-1001",
                CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);

        Assert.Equal(
            "P-1001",
            result.PartnerId);

        Assert.Equal(
            3,
            handler.CallCount);
    }

    [Fact]
    public async Task VerifyAsync_Should_Fail_After_MaxRetryAttempts()
    {
        // Arrange
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var services = new ServiceCollection();

        services.AddLogging();

        services
            .AddHttpClient<
                IPartnerVerificationClient,
                PartnerVerificationClient>(client =>
            {
                client.BaseAddress =
                    new Uri("http://localhost");
            })
            .ConfigurePrimaryHttpMessageHandler(
                () => handler)
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 2;

                options.Retry.Delay =
                    TimeSpan.Zero;

                options.Retry.UseJitter =
                    false;

                options.AttemptTimeout.Timeout =
                    TimeSpan.FromSeconds(2);

                options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(5);
            });

        await using var provider =
            services.BuildServiceProvider();

        var client =
            provider.GetRequiredService<
                IPartnerVerificationClient>();

        // Act
        var action = () =>
            client.VerifyAsync(
                "P-1001",
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<
            PartnerVerificationUnavailableException>(
                action);

        Assert.Equal(
            3,
            handler.CallCount);
    }

    [Fact]
    public async Task VerifyAsync_Should_NotRetry_When_ResponseIsBadRequest()
    {
        // Arrange
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.BadRequest));

        var services = new ServiceCollection();

        services.AddLogging();

        services
            .AddHttpClient<
                IPartnerVerificationClient,
                PartnerVerificationClient>(client =>
            {
                client.BaseAddress =
                    new Uri("http://localhost");
            })
            .ConfigurePrimaryHttpMessageHandler(
                () => handler)
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 2;

                options.Retry.Delay =
                    TimeSpan.Zero;

                options.Retry.UseJitter =
                    false;

                options.AttemptTimeout.Timeout =
                    TimeSpan.FromSeconds(2);

                options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(5);
            });

        await using var provider =
            services.BuildServiceProvider();

        var client =
            provider.GetRequiredService<
                IPartnerVerificationClient>();

        // Act
        var action = () =>
            client.VerifyAsync(
                "P-1001",
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<
            PartnerVerificationUnavailableException>(
                action);

        Assert.Equal(
            1,
            handler.CallCount);
    }

    private static HttpResponseMessage CreateSuccessResponse()
    {
        const string json = """
        {
            "partnerId": "P-1001",
            "isValid": true
        }
        """;

        return new HttpResponseMessage(
            HttpStatusCode.OK)
        {
            Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json")
        };
    }

    private sealed class SequenceHttpMessageHandler
        : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage>
            _responses;

        public int CallCount { get; private set; }

        public SequenceHttpMessageHandler(
            params HttpResponseMessage[] responses)
        {
            _responses =
                new Queue<HttpResponseMessage>(
                    responses);
        }

        protected override Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            CallCount++;

            if (_responses.Count == 0)
            {
                throw new InvalidOperationException(
                    "No response configured.");
            }

            return Task.FromResult(
                _responses.Dequeue());
        }
    }
}