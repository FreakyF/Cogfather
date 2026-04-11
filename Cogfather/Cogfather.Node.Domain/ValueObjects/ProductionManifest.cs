namespace Cogfather.Node.Domain.ValueObjects;

public record ProductionManifest(Guid CorrelationId, string ComponentId, int Amount)
{
    public static ProductionManifest Create(Guid correlationId, string componentId, int amount)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId cannot be empty.", nameof(correlationId));
        if (string.IsNullOrWhiteSpace(componentId))
            throw new ArgumentException("ComponentId cannot be null or whitespace.", nameof(componentId));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");

        return new ProductionManifest(correlationId, componentId, amount);
    }
}