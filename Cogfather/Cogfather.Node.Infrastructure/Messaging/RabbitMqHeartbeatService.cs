using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cogfather.Contracts;
using Cogfather.Contracts.Messages.Heartbeat;
using Cogfather.Node.Application.Interfaces;
using Cogfather.Node.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cogfather.Node.Infrastructure.Messaging;

public class RabbitMqHeartbeatService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IInventoryStore _inventoryStore;
    private readonly ILogger<RabbitMqHeartbeatService> _logger;
    private readonly NodeRabbitMqOptions _options;
    private readonly Guid _nodeGuid;
    private readonly string _displayName;

    public RabbitMqHeartbeatService(
        IConnection connection,
        IOptions<NodeRabbitMqOptions> options,
        IInventoryStore inventoryStore,
        IConfiguration configuration,
        ILogger<RabbitMqHeartbeatService> logger)
    {
        _connection = connection;
        _options = options.Value;
        _inventoryStore = inventoryStore;
        _logger = logger;
        _displayName = configuration["NodeSettings:NodeId"] ?? "UnknownNode";
        _nodeGuid = ToNodeGuid(_displayName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.HeartbeatIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendHeartbeatAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send heartbeat");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        var policy = PollyPolicies.GetRetryPolicy(_logger);

        await policy.ExecuteAsync(async () =>
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(_options.HeartbeatQueue, true, false, false,
                cancellationToken: cancellationToken);

            var snapshot = _inventoryStore.GetInventory().GetAll();

            var message = new NodeHeartbeatMessage(
                _nodeGuid,
                _displayName,
                DateTimeOffset.UtcNow,
                snapshot);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message,
                CogfatherJsonContext.Default.NodeHeartbeatMessage));
            var properties = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(string.Empty, _options.HeartbeatQueue, true, properties, body,
                cancellationToken);

            _logger.LogDebug("Heartbeat sent for node {NodeId}", _displayName);
        });
    }

    private static Guid ToNodeGuid(string nodeId)
    {
        if (Guid.TryParse(nodeId, out var guid)) return guid;
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(nodeId)));
    }
}