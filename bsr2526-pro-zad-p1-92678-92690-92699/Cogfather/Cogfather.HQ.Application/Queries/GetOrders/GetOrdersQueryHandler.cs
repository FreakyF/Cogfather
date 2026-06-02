using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IEnumerable<ProductionOrder>>
{
    private readonly IProductionOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IProductionOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Task<IEnumerable<ProductionOrder>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        return _orderRepository.GetAllAsync(cancellationToken);
    }
}