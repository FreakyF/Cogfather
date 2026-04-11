namespace Cogfather.Node.Domain.Records;

public record ProductionResult(bool Success, string ComponentId, int AmountProduced, string ErrorMessage = null)
{
    public static ProductionResult Successful(string componentId, int amountProduced) 
        => new(true, componentId, amountProduced);
        
    public static ProductionResult Failed(string componentId, int attemptedAmount, string errorMessage) 
        => new(false, componentId, attemptedAmount, errorMessage);
}
