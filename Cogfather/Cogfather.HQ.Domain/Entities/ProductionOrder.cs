using Cogfather.HQ.Domain.Enums;

namespace Cogfather.HQ.Domain.Entities;

public class ProductionOrder
{
    public ProductionOrder(string recipeId, double targetAmount)
    {
        Id = Guid.NewGuid();
        RecipeId = recipeId;
        TargetAmount = targetAmount;
        Status = ProductionOrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string RecipeId { get; private set; }
    public double TargetAmount { get; private set; }
    public ProductionOrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void StartProduction()
    {
        if (Status != ProductionOrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be started.");
        Status = ProductionOrderStatus.InProgress;
    }

    public void CompleteProduction()
    {
        if (Status != ProductionOrderStatus.InProgress)
            throw new InvalidOperationException("Only in-progress orders can be completed.");
        Status = ProductionOrderStatus.Completed;
    }

    public void FailProduction()
    {
        Status = ProductionOrderStatus.Failed;
    }

    public void CancelProduction()
    {
        if (Status == ProductionOrderStatus.Completed || Status == ProductionOrderStatus.Failed)
            throw new InvalidOperationException("Cannot cancel a completed or failed order.");
        Status = ProductionOrderStatus.Cancelled;
    }
}