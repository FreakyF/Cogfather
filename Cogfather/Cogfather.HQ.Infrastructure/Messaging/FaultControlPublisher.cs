using System.Text;
using System.Text.Json;
using Cogfather.Contracts;
using Cogfather.Contracts.Enums;
using Cogfather.Contracts.Messages.Faults;
using Cogfather.HQ.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cogfather.HQ.Infrastructure.Messaging;

public class FaultControlPublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<FaultControlPublisher> _logger;
    private readonly RabbitMqOptions _options;

    public FaultControlPublisher(IConnection connection, IOptions<RabbitMqOptions> options,
        ILogger<FaultControlPublisher> logger)
    {
        _connection = connection;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(string nodeId, FaultModeContract faultMode, int delaySeconds,
        CancellationToken cancellationToken = default)
    {
        var policy = PollyPolicies.GetRetryPolicy(_logger);

        await policy.ExecuteAsync(async () =>
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                _options.FaultsExchange,
                ExchangeType.Direct,
                true,
                cancellationToken: cancellationToken);

            var message = new FaultControlMessage(
                Guid.Parse(nodeId),
                faultMode,
                delaySeconds);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, CogfatherJsonContext.Default.FaultControlMessage));
            var properties = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                _options.FaultsExchange,
                nodeId,
                true,
                properties,
                body,
                cancellationToken);

            _logger.LogInformation("Dispatched fault control {FaultMode} to node {NodeId}", faultMode, nodeId);
        });
    }
}