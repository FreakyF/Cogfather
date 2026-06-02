using Cogfather.HQ.Domain.Enums;

namespace Cogfather.HQ.Domain.Entities;

/// <summary>
/// Represents a single production order issued by HQ to worker nodes.
/// Transitions: Pending → InProgress → Completed | Failed | Cancelled.
/// </summary>
public class ProductionOrder
{
    /// <summary>Creates a new order in <see cref="ProductionOrderStatus.Pending"/> state.</summary>
    public ProductionOrder(string recipeId, double targetAmount)
    {
        Id = Guid.NewGuid();
        RecipeId = recipeId;
        TargetAmount = targetAmount;
        Status = ProductionOrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Unique order identifier (correlation ID shared with worker nodes).</summary>
    public Guid Id { get; private set; }

    /// <summary>The recipe being produced.</summary>
    public string RecipeId { get; private set; }

    /// <summary>Requested output quantity.</summary>
    public double TargetAmount { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ProductionOrderStatus Status { get; private set; }

    /// <summary>UTC timestamp when the order was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Transitions the order from <see cref="ProductionOrderStatus.Pending"/> to <see cref="ProductionOrderStatus.InProgress"/>.</summary>
    /// <exception cref="InvalidOperationException">Thrown if the order is not pending.</exception>
    public void StartProduction()
    {
        if (Status != ProductionOrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be started.");
        Status = ProductionOrderStatus.InProgress;
    }

    /// <summary>Marks the order as successfully completed by consensus.</summary>
    /// <exception cref="InvalidOperationException">Thrown if the order is not in progress.</exception>
    public void CompleteProduction()
    {
        if (Status != ProductionOrderStatus.InProgress)
            throw new InvalidOperationException("Only in-progress orders can be completed.");
        Status = ProductionOrderStatus.Completed;
    }

    /// <summary>Marks the order as failed (e.g. Byzantine fault detected by consensus).</summary>
    public void FailProduction()
    {
        Status = ProductionOrderStatus.Failed;
    }

    /// <summary>Cancels the order if it has not already completed or failed.</summary>
    /// <exception cref="InvalidOperationException">Thrown if the order is completed or failed.</exception>
    public void CancelProduction()
    {
        if (Status == ProductionOrderStatus.Completed || Status == ProductionOrderStatus.Failed)
            throw new InvalidOperationException("Cannot cancel a completed or failed order.");
        Status = ProductionOrderStatus.Cancelled;
    }
}
