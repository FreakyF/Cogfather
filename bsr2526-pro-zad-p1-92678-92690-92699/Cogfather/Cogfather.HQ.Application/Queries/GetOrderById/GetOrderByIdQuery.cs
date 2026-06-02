using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<ProductionOrder?>;