namespace Cogfather.HQ.Domain.Entities;

public class ProductionReport
{
    public ProductionReport(string nodeId, string recipeId, bool success)
    {
        Id = Guid.NewGuid();
        NodeId = nodeId;
        RecipeId = recipeId;
        Success = success;
        ReportedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string NodeId { get; private set; }
    public string RecipeId { get; private set; }
    public bool Success { get; private set; }
    public DateTime ReportedAt { get; private set; }
}