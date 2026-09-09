using System.Text.Json;
using Microsoft.Extensions.Options;
using PartnerIntegration.Api.Configuration;
using PartnerIntegration.Api.Exceptions;
using RabbitMQ.Client;

namespace PartnerIntegration.Api.Messaging;

public sealed class RabbitMqMessagePublisher
    : IMessagePublisher,
      IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqMessagePublisher> _logger;

    private readonly SemaphoreSlim _initializationLock =
        new(1, 1);

    private readonly SemaphoreSlim _publishLock =
        new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqMessagePublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqMessagePublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(
        PartnerTransactionMessage message,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);

        var body =
            JsonSerializer.SerializeToUtf8Bytes(message);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            Persistent = true,
            MessageId = message.TransactionReference
        };

        using var timeoutCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeoutCts.CancelAfter(
            TimeSpan.FromSeconds(
                _options.PublishTimeoutSeconds));

        await _publishLock.WaitAsync(cancellationToken);

        try
        {
            await _channel!.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.QueueName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: timeoutCts.Token);

            _logger.LogInformation(
                "Transaction {TransactionReference} published to queue {QueueName}",
                message.TransactionReference,
                _options.QueueName);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MessagePublishingException(
                "Failed to publish transaction to message broker.",
                ex);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    private async Task EnsureInitializedAsync(
        CancellationToken cancellationToken)
    {
        if (_connection is not null &&
            _channel is not null)
        {
            return;
        }

        await _initializationLock.WaitAsync(
            cancellationToken);

        try
        {
            if (_connection is not null &&
                _channel is not null)
            {
                return;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,

                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            _connection =
                await factory.CreateConnectionAsync(
                    "partner-integration-publisher",
                    cancellationToken);

            var channelOptions =
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true);

            _channel =
                await _connection.CreateChannelAsync(
                    channelOptions,
                    cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _initializationLock.Dispose();
        _publishLock.Dispose();
    }
}