using MediatR;

namespace Cogfather.HQ.Application.Commands.ReceiveProductionReport;

public record ReceiveProductionReportCommand(
    Guid OrderId,
    string NodeId,
    string RecipeId,
    bool Success
) : IRequest;