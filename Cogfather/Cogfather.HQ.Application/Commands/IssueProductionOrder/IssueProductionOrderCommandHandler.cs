using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Exceptions;
using MediatR;

namespace Cogfather.HQ.Application.Commands.IssueProductionOrder;

public class IssueProductionOrderCommandHandler : IRequestHandler<IssueProductionOrderCommand, Guid>
{
    private readonly IProductionCatalog _catalog;
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IOrderDispatcher _dispatcher;

    public IssueProductionOrderCommandHandler(
        IProductionCatalog catalog,
        IProductionOrderRepository orderRepository,
        IOrderDispatcher dispatcher)
    {
        _catalog = catalog;
        _orderRepository = orderRepository;
        _dispatcher = dispatcher;
    }

    public async Task<Guid> Handle(IssueProductionOrderCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _catalog.GetRecipeAsync(request.RecipeId, cancellationToken)
                     ?? throw new RecipeNotFoundException(request.RecipeId);

        var order = new ProductionOrder(request.RecipeId, request.TargetAmount);

        await _orderRepository.AddAsync(order, cancellationToken);

        order.StartProduction();
        await _orderRepository.UpdateAsync(order, cancellationToken);

        await _dispatcher.DispatchAsync(order, recipe, cancellationToken);

        return order.Id;
    }
}