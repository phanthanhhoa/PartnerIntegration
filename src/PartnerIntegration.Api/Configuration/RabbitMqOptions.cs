namespace PartnerIntegration.Api.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string QueueName { get; init; } =
        "partner-transactions";

    public int PublishTimeoutSeconds { get; init; } = 5;
}