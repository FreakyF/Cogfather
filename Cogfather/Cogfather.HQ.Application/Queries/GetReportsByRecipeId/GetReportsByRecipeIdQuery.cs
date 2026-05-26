using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetReportsByRecipeId;

public record GetReportsByRecipeIdQuery(string RecipeId) : IRequest<IEnumerable<ProductionReport>>;
