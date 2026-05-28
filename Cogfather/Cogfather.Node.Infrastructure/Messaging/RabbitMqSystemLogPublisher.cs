using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cogfather.Contracts;
using Cogfather.Contracts.Events;
using Cogfather.Node.Application.Interfaces;
using Cogfather.Node.Domain.ValueObjects;
using Cogfather.Node.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cogfather.Node.Infrastructure.Messaging;

public class RabbitMqSystemLogPublisher : ISystemLogPublisher
{
    private readonly IConnection _connection;
    private readonly NodeRabbitMqOptions _options;
    private readonly string _nodeId;
    private readonly ILogger<RabbitMqSystemLogPublisher> _logger;

    public RabbitMqSystemLogPublisher(
        IConnection connection,
        IOptions<NodeRabbitMqOptions> options,
        NodeIdentity identity,
        ILogger<RabbitMqSystemLogPublisher> logger)
    {
        _connection = connection;
        _options = options.Value;
        _nodeId = identity.NodeId;
        _logger = logger;
    }

    public async Task PublishAsync(string level, string category, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = PollyPolicies.GetRetryPolicy(_logger);
            await policy.ExecuteAsync(async () =>
            {
                await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
                await channel.ExchangeDeclareAsync(_options.LogsExchange, ExchangeType.Fanout, true,
                    cancellationToken: cancellationToken);

                var evt = new SystemLogEvent(_nodeId, level, $"[{category}] {message}", DateTime.UtcNow);
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt, CogfatherJsonContext.Default.SystemLogEvent));
                var props = new BasicProperties { Persistent = false };

                await channel.BasicPublishAsync(_options.LogsExchange, string.Empty, false, props, body,
                    cancellationToken);
            });
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to publish system log event");
        }
    }

    private static Guid ToNodeGuid(string nodeId)
    {
        if (Guid.TryParse(nodeId, out var guid)) return guid;
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(nodeId)));
    }
}
