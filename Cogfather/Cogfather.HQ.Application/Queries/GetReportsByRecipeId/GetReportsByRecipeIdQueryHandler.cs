using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetReportsByRecipeId;

public class GetReportsByRecipeIdQueryHandler : IRequestHandler<GetReportsByRecipeIdQuery, IEnumerable<ProductionReport>>
{
    private readonly IProductionReportRepository _reports;

    public GetReportsByRecipeIdQueryHandler(IProductionReportRepository reports)
    {
        _reports = reports;
    }

    public Task<IEnumerable<ProductionReport>> Handle(GetReportsByRecipeIdQuery request, CancellationToken cancellationToken)
        => _reports.GetByRecipeIdAsync(request.RecipeId, cancellationToken);
}
