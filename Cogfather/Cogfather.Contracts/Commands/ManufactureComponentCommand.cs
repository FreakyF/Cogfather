namespace Cogfather.Contracts.Commands;

public record ManufactureComponentCommand(
    Guid CorrelationId,
    string ComponentId,
    int Amount
);