using MediatR;

namespace Cogfather.HQ.Application.Commands.ReceiveProductionReport;

public record ReceiveProductionReportCommand(
    string NodeId,
    string RecipeId,
    bool Success
) : IRequest;