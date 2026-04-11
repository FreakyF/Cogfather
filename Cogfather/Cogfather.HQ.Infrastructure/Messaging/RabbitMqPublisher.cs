using System.Text;
using System.Text.Json;
using Cogfather.Contracts.Messages.Orders;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.ValueObjects;
using Cogfather.HQ.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cogfather.HQ.Infrastructure.Messaging;

public class RabbitMqPublisher : IOrderDispatcher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqOptions _options;

    public RabbitMqPublisher(IConnection connection, IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _options = options.Value;
        _logger = logger;
    }

    public async Task DispatchAsync(ProductionOrder order, Recipe recipe, CancellationToken cancellationToken = default)
    {
        var policy = PollyPolicies.GetRetryPolicy(_logger);

        await policy.ExecuteAsync(async () =>
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                _options.OrdersExchange,
                ExchangeType.Fanout,
                true,
                cancellationToken: cancellationToken);

            var recipeJson = JsonSerializer.Serialize(recipe);
            var message = new ProductionOrderMessage(
                order.Id,
                order.Id,
                order.RecipeId,
                (int)order.TargetAmount,
                DateTimeOffset.UtcNow,
                recipeJson);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                _options.OrdersExchange,
                string.Empty,
                true,
                properties,
                body,
                cancellationToken);

            _logger.LogInformation("Dispatched order {OrderId} for {RecipeId}", order.Id, order.RecipeId);
        });
    }
}