using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetReportsByCorrelationId;

public record GetReportsByCorrelationIdQuery(Guid CorrelationId) : IRequest<IEnumerable<ProductionReport>>;
