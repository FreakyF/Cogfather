using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cogfather.Contracts;
using Cogfather.Contracts.Messages.Reports;
using Cogfather.Node.Application.Interfaces;
using Cogfather.Node.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cogfather.Node.Infrastructure.Messaging;

public class RabbitMqReportPublisher : IReportPublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqReportPublisher> _logger;
    private readonly NodeRabbitMqOptions _options;

    public RabbitMqReportPublisher(
        IConnection connection,
        IOptions<NodeRabbitMqOptions> options,
        ILogger<RabbitMqReportPublisher> logger)
    {
        _connection = connection;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishProductionReportAsync(Guid correlationId, string nodeId, string componentId,
        int amountProduced, bool success, string hash, string errorMessage = null)
    {
        var policy = PollyPolicies.GetRetryPolicy(_logger);

        await policy.ExecuteAsync(async () =>
        {
            await using var channel = await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(_options.ReportsExchange, ExchangeType.Fanout, true);

            var message = new ProductionReportMessage(
                Guid.NewGuid(),
                correlationId,
                ToNodeGuid(nodeId),
                componentId,
                amountProduced,
                hash,
                !success,
                DateTimeOffset.UtcNow);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message,
                CogfatherJsonContext.Default.ProductionReportMessage));
            var properties = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(_options.ReportsExchange, string.Empty, true, properties, body);

            _logger.LogInformation("Published production report for CorrelationId {CorrelationId}", correlationId);
        });
    }

    private static Guid ToNodeGuid(string nodeId)
    {
        if (Guid.TryParse(nodeId, out var guid)) return guid;
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(nodeId)));
    }
}