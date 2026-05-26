using System.Text;
using System.Text.Json;
using Cogfather.Contracts;
using Cogfather.Contracts.Messages.Orders;
using Cogfather.Node.Application.Commands;
using Cogfather.Node.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cogfather.Node.Infrastructure.Messaging;

public class RabbitMqOrderConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqOrderConsumerService> _logger;
    private readonly NodeRabbitMqOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _nodeId;
    private IChannel? _channel;

    public RabbitMqOrderConsumerService(
        IConnection connection,
        IOptions<NodeRabbitMqOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        NodeIdentity identity,
        ILogger<RabbitMqOrderConsumerService> logger)
    {
        _connection = connection;
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _nodeId = identity.NodeId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(_options.OrdersExchange, ExchangeType.Fanout, true,
            cancellationToken: stoppingToken);

        var queueName = $"cogfather.orders.{_nodeId}";
        await _channel.QueueDeclareAsync(queueName, true, false, false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(queueName, _options.OrdersExchange, string.Empty,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);

            try
            {
                var order = JsonSerializer.Deserialize(messageString,
                    CogfatherJsonContext.Default.ProductionOrderMessage);
                if (order != null)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var energy = 0.0;
                    if (!string.IsNullOrEmpty(order.RecipeJson))
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(order.RecipeJson);
                        if (doc.RootElement.TryGetProperty("Energy", out var energyProp) ||
                            doc.RootElement.TryGetProperty("energy", out energyProp))
                            energy = energyProp.GetDouble();
                    }

                    var command = new ExecuteProductionOrderCommand(
                        order.CorrelationId,
                        order.RecipeId,
                        order.RequestedQuantity,
                        energy);

                    await mediator.Send(command, stoppingToken);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order message");
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
}