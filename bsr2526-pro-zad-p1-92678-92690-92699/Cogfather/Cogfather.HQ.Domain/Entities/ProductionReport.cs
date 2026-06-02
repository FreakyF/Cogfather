namespace Cogfather.HQ.Domain.Entities;

/// <summary>
/// A single production report submitted by a worker node after attempting to execute an order.
/// Multiple reports for the same <see cref="CorrelationId"/> are aggregated by the consensus engine.
/// </summary>
public class ProductionReport
{
    /// <summary>Creates a production report.</summary>
    /// <param name="correlationId">Order ID this report belongs to.</param>
    /// <param name="nodeId">UUID of the reporting node.</param>
    /// <param name="recipeId">Recipe that was executed.</param>
    /// <param name="success">Whether production succeeded on this node.</param>
    public ProductionReport(Guid correlationId, string nodeId, string recipeId, bool success)
    {
        Id = Guid.NewGuid();
        CorrelationId = correlationId;
        NodeId = nodeId;
        RecipeId = recipeId;
        Success = success;
        ReportedAt = DateTime.UtcNow;
    }

    /// <summary>Unique report identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>The production order this report relates to.</summary>
    public Guid CorrelationId { get; private set; }

    /// <summary>UUID of the worker node that produced this report.</summary>
    public string NodeId { get; private set; }

    /// <summary>Recipe identifier executed by the node.</summary>
    public string RecipeId { get; private set; }

    /// <summary><see langword="true"/> if the node reported successful production; <see langword="false"/> for failure.</summary>
    public bool Success { get; private set; }

    /// <summary>UTC timestamp when the report was received by HQ.</summary>
    public DateTime ReportedAt { get; private set; }
}
