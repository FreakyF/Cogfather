using MediatR;

namespace Cogfather.HQ.Application.Commands.IssueProductionOrder;

public record IssueProductionOrderCommand(string RecipeId, double TargetAmount) : IRequest<Guid>;