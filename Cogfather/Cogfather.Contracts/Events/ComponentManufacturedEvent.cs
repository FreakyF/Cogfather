namespace Cogfather.Contracts.Events;

public record ComponentManufacturedEvent(
    Guid CorrelationId,
    string NodeId,
    string ComponentId,
    int AmountProduced,
    string ManifestHash,
    DateTime Timestamp
);