namespace Cogfather.HQ.Domain.Exceptions;

public class InsufficientInventoryException : Exception
{
    public InsufficientInventoryException(string componentId, double requiredAmount)
        : base($"Insufficient inventory for component '{componentId}'. Required amount: {requiredAmount}.")
    {
        ComponentId = componentId;
        RequiredAmount = requiredAmount;
    }

    public string ComponentId { get; }
    public double RequiredAmount { get; }
}