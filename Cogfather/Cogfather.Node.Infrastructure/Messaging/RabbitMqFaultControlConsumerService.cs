using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cogfather.Contracts.Messages.Faults;
using Cogfather.Node.Domain.Enums;
using Cogfather.Node.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cogfather.Node.Infrastructure.Messaging;

public class RabbitMqFaultControlConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IFaultInjector _faultInjector;
    private readonly ILogger<RabbitMqFaultControlConsumerService> _logger;
    private readonly NodeRabbitMqOptions _options;
    private readonly string _routingKey;
    private IChannel? _channel;

    public RabbitMqFaultControlConsumerService(
        IConnection connection,
        IOptions<NodeRabbitMqOptions> options,
        IFaultInjector faultInjector,
        IConfiguration configuration,
        ILogger<RabbitMqFaultControlConsumerService> logger)
    {
        _connection = connection;
        _options = options.Value;
        _faultInjector = faultInjector;
        _logger = logger;
        var nodeId = configuration["NodeSettings:NodeId"] ?? "UnknownNode";
        _routingKey = ToNodeGuid(nodeId).ToString();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(_options.FaultsExchange, ExchangeType.Direct, true,
            cancellationToken: stoppingToken);

        var queueName = $"cogfather.faults.{_routingKey}";
        await _channel.QueueDeclareAsync(queueName, true, false, false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(queueName, _options.FaultsExchange, _routingKey,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);

            try
            {
                var message = JsonSerializer.Deserialize<FaultControlMessage>(messageString);
                if (message != null)
                {
                    var faultMode = (FaultMode)(int)message.FaultMode;
                    _faultInjector.SetFaultMode(faultMode);
                    _logger.LogInformation("Fault mode set to {FaultMode} for node {NodeId}",
                        faultMode, _routingKey);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fault control message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(queueName, false, consumer, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private static Guid ToNodeGuid(string nodeId)
    {
        if (Guid.TryParse(nodeId, out var guid)) return guid;
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(nodeId)));
    }
}