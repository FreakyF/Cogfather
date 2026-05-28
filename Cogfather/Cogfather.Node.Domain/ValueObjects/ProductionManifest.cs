namespace Cogfather.Node.Domain.ValueObjects;

/// <summary>
/// Immutable descriptor of a production task sent to a worker node.
/// Created from an HQ order and hashed for integrity verification.
/// </summary>
/// <param name="CorrelationId">Matches the HQ production order ID.</param>
/// <param name="ComponentId">Recipe/component to produce.</param>
/// <param name="Amount">Number of units to produce.</param>
/// <param name="Energy">Energy cost declared in the recipe (used for hash).</param>
public record ProductionManifest(Guid CorrelationId, string ComponentId, int Amount, double Energy = 0.0)
{
    /// <summary>Creates a validated <see cref="ProductionManifest"/>.</summary>
    public static ProductionManifest Create(Guid correlationId, string componentId, int amount, double energy = 0.0)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId cannot be empty.", nameof(correlationId));
        if (string.IsNullOrWhiteSpace(componentId))
            throw new ArgumentException("ComponentId cannot be null or whitespace.", nameof(componentId));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");

        return new ProductionManifest(correlationId, componentId, amount, energy);
    }
}