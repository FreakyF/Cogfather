using System.Text;
using System.Text.Json;
using Cogfather.Contracts;
using Cogfather.HQ.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cogfather.HQ.Infrastructure.Messaging;

public class RabbitMqLogConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqLogConsumerService> _logger;
    private readonly RabbitMqOptions _options;
    private readonly SystemLogService _logService;
    private IChannel? _channel;

    public RabbitMqLogConsumerService(
        IConnection connection,
        IOptions<RabbitMqOptions> options,
        SystemLogService logService,
        ILogger<RabbitMqLogConsumerService> logger)
    {
        _connection = connection;
        _options = options.Value;
        _logService = logService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(_options.LogsExchange, ExchangeType.Fanout, true,
            cancellationToken: stoppingToken);
        await _channel.QueueDeclareAsync(_options.LogsQueue, false, false, true, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(_options.LogsQueue, _options.LogsExchange, string.Empty,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize(json, CogfatherJsonContext.Default.SystemLogEvent);
                if (evt != null)
                {
                    var parts = evt.Message.Split(']', 2);
                    var category = parts.Length == 2 ? parts[0].TrimStart('[') : "Node";
                    var message = parts.Length == 2 ? parts[1].TrimStart() : evt.Message;

                    _logService.Add(new SystemLogEntry
                    {
                        Source = evt.NodeId,
                        Level = evt.Level,
                        Category = category,
                        Message = message,
                        Timestamp = evt.Timestamp
                    });
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error processing log event");
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(_options.LogsQueue, false, consumer, stoppingToken);
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
