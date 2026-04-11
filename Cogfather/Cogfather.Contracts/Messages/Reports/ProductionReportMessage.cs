namespace Cogfather.Contracts.Messages.Reports;

/// <summary>
/// Sent by a Node after completing (or failing) a production run.
/// HQ groups these by CorrelationId for consensus evaluation.
/// </summary>
public sealed record ProductionReportMessage(
    /// <summary>Unique ID of this report.</summary>
    Guid ReportId,
    /// <summary>
    /// Must match the CorrelationId from the original ProductionOrderMessage.
    /// </summary>
    Guid CorrelationId,
    /// <summary>The node that produced this report.</summary>
    Guid NodeId,
    /// <summary>Recipe that was executed.</summary>
    string RecipeId,
    /// <summary>
    /// Actual quantity produced. May differ from requested due to
    /// probability rolls or injected faults.
    /// </summary>
    int ProducedQuantity,
    /// <summary>
    /// SHA-256 hex string of the canonical manifest payload.
    /// Canonical format: "{CorrelationId}|{NodeId}|{RecipeId}|{ProducedQuantity}|{ProducedAt:O}"
    /// </summary>
    string ManifestHash,
    /// <summary>
    /// Whether the node claimed it could not fulfil the order
    /// due to insufficient inventory.
    /// </summary>
    bool InsufficientInventory,
    /// <summary>UTC timestamp when production completed.</summary>
    DateTimeOffset ProducedAt
);