using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ProductionOrder?>
{
    private readonly IProductionOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IProductionOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Task<ProductionOrder?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return _orderRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}