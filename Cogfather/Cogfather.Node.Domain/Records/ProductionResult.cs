namespace Cogfather.Node.Domain.Records;

public record ProductionResult(bool Success, string ComponentId, int AmountProduced, string ErrorMessage = null)
{
    public static ProductionResult Successful(string componentId, int amountProduced)
    {
        return new ProductionResult(true, componentId, amountProduced);
    }

    public static ProductionResult Failed(string componentId, int attemptedAmount, string errorMessage)
    {
        return new ProductionResult(false, componentId, attemptedAmount, errorMessage);
    }
}