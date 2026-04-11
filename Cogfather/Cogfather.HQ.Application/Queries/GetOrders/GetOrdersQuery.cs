using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetOrders;

public record GetOrdersQuery : IRequest<IEnumerable<ProductionOrder>>;