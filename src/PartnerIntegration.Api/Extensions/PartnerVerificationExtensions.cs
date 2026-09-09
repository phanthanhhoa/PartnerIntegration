using PartnerIntegration.Api.Clients;
using Polly;

namespace PartnerIntegration.Api.Extensions;

public static class PartnerVerificationExtensions
{
    public static IServiceCollection
        AddPartnerVerification(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services
            .AddHttpClient<
                IPartnerVerificationClient,
                PartnerVerificationClient>(
                client =>
                {
                    var baseUrl =
                        configuration["PartnerVerification:BaseUrl"]
                        ?? throw new InvalidOperationException("PartnerVerification BaseUrl is missing.");

                    client.BaseAddress = new Uri(baseUrl);
                })
            .AddStandardResilienceHandler(
                options =>
                {
                    options.Retry.MaxRetryAttempts = 3;

                    options.Retry.Delay = TimeSpan.FromMilliseconds(200);

                    options.Retry.BackoffType = DelayBackoffType.Exponential;

                    options.Retry.UseJitter = true;

                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);

                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
                });

        return services;
    }
}