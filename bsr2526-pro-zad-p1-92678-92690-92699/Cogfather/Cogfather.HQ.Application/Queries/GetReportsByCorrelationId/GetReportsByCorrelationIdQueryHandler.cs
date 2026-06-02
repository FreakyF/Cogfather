using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetReportsByCorrelationId;

public class GetReportsByCorrelationIdQueryHandler
    : IRequestHandler<GetReportsByCorrelationIdQuery, IEnumerable<ProductionReport>>
{
    private readonly IProductionReportRepository _reports;

    public GetReportsByCorrelationIdQueryHandler(IProductionReportRepository reports)
    {
        _reports = reports;
    }

    public Task<IEnumerable<ProductionReport>> Handle(
        GetReportsByCorrelationIdQuery request,
        CancellationToken cancellationToken)
        => _reports.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
}
