namespace Cogfather.Contracts.Messages.Orders;

/// <summary>
/// Message sent by HQ to all Worker Nodes to initiate a production run.
/// Published once per order; all nodes receive via fanout.
/// </summary>
public sealed record ProductionOrderMessage(
    /// <summary>Unique ID of this order.</summary>
    Guid OrderId,
    /// <summary>
    /// Correlation ID used to group reports from all nodes for this order.
    /// Must be included unchanged in every resulting ProductionReportMessage.
    /// </summary>
    Guid CorrelationId,
    /// <summary>Recipe identifier, e.g. "wooden-chest".</summary>
    string RecipeId,
    /// <summary>How many units to produce.</summary>
    int RequestedQuantity,
    /// <summary>UTC timestamp when HQ issued this order.</summary>
    DateTimeOffset IssuedAt,
    /// <summary>
    /// Serialized recipe payload (JSON). Nodes must not trust any local state
    /// for recipe definitions — use this embedded copy only.
    /// </summary>
    string RecipeJson
);