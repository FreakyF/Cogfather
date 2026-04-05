using Cogfather.Contracts.Events;
using JetBrains.Annotations;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Cogfather.HQ.Infrastructure.Consumers;

[UsedImplicitly]
public partial class ComponentManufacturedConsumer(ILogger<ComponentManufacturedConsumer> logger) 
    : IConsumer<ComponentManufacturedEvent>
{
    [LoggerMessage(Level = LogLevel.Information, Message = "[HQ] Report received from {NodeId}: {Amount}x {ComponentId} (Hash: {Hash})")]
    private static partial void LogReportReceived(ILogger logger, string nodeId, double amount, string componentId, string hash);

    public Task Consume(ConsumeContext<ComponentManufacturedEvent> context)
    {
        var e = context.Message;
        LogReportReceived(logger, e.NodeId, e.AmountProduced, e.ComponentId, e.ManifestHash);
        
        return Task.CompletedTask;
    }
}