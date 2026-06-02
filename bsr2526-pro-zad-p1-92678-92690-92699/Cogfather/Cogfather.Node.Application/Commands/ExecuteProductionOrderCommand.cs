using MediatR;

namespace Cogfather.Node.Application.Commands;

public record ExecuteProductionOrderCommand(
    Guid CorrelationId,
    string ComponentId,
    int Amount,
    double Energy = 0.0
) : IRequest;