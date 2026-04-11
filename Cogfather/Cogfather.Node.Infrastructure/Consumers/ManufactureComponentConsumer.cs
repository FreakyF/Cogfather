using Cogfather.Contracts.Commands;
using Cogfather.Contracts.Events;
using JetBrains.Annotations;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cogfather.Node.Infrastructure.Consumers;

[UsedImplicitly]
public partial class ManufactureComponentConsumer(
    ILogger<ManufactureComponentConsumer> logger,
    IConfiguration config)
    : IConsumer<ManufactureComponentCommand>
{
    private readonly string _nodeId = config["NodeSettings:NodeId"] ?? "Unknown-Node";

    [LoggerMessage(Level = LogLevel.Information,
        Message = "[{Node}] Received order: {Amount}x {ComponentId} (Task: {CorrelationId})")]
    private static partial void LogOrderReceived(ILogger logger, string node, double amount, string componentId,
        Guid correlationId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "[{Node}] Production finished for {CorrelationId}. Reporting back.")]
    private static partial void LogProductionFinished(ILogger logger, string node, Guid correlationId);

    public async Task Consume(ConsumeContext<ManufactureComponentCommand> context)
    {
        var (correlationId, componentId, amount) = context.Message;

        LogOrderReceived(logger, _nodeId, amount, componentId, correlationId);

        await Task.Delay(2000);

        var manifestHash = Guid.NewGuid().ToString("N").Substring(0, 8);

        LogProductionFinished(logger, _nodeId, correlationId);

        await context.Publish(new ComponentManufacturedEvent(
            correlationId,
            _nodeId,
            componentId,
            amount,
            manifestHash,
            DateTime.UtcNow
        ));
    }
}