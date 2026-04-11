using System.Text;
using System.Text.Json;
using Cogfather.Contracts.Messages.Reports;
using Cogfather.HQ.Application.Commands.ReceiveProductionReport;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cogfather.HQ.Infrastructure.Messaging;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IChannel? _channel;

    public RabbitMqConsumerService(
        IConnection connection,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RabbitMqConsumerService> logger)
    {
        _connection = connection;
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(_options.ReportsExchange, ExchangeType.Fanout, true,
            cancellationToken: stoppingToken);
        await _channel.QueueDeclareAsync(_options.ReportsQueue, true, false, false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(_options.ReportsQueue, _options.ReportsExchange, string.Empty,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);

            try
            {
                var report = JsonSerializer.Deserialize<ProductionReportMessage>(messageString);
                if (report != null)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var command = new ReceiveProductionReportCommand(
                        report.NodeId.ToString(),
                        report.RecipeId,
                        !report.InsufficientInventory && report.ProducedQuantity > 0
                    );

                    await mediator.Send(command, stoppingToken);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing report message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(_options.ReportsQueue, false, consumer, stoppingToken);
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