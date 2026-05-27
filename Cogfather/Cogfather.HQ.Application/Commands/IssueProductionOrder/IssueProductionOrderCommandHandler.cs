using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Exceptions;
using Cogfather.HQ.Domain.ValueObjects;
using MediatR;

namespace Cogfather.HQ.Application.Commands.IssueProductionOrder;

public class IssueProductionOrderCommandHandler : IRequestHandler<IssueProductionOrderCommand, Guid>
{
    private readonly IProductionCatalog _catalog;
    private readonly IOrderDispatcher _dispatcher;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductionOrderRepository _orderRepository;

    public IssueProductionOrderCommandHandler(
        IProductionCatalog catalog,
        IProductionOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IOrderDispatcher dispatcher)
    {
        _catalog = catalog;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _dispatcher = dispatcher;
    }

    public async Task<Guid> Handle(IssueProductionOrderCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _catalog.GetRecipeAsync(request.RecipeId, cancellationToken)
                     ?? throw new RecipeNotFoundException(request.RecipeId);

        var inventory = await _inventoryRepository.GetAsync(cancellationToken);

        var issued = new HashSet<string>();
        await IssueSubOrdersAsync(recipe, request.TargetAmount, new HashSet<string>(), issued, inventory, cancellationToken);

        await _inventoryRepository.SaveAsync(inventory, cancellationToken);

        return await IssueOrderAsync(recipe, request.TargetAmount, cancellationToken);
    }

    private async Task IssueSubOrdersAsync(
        Recipe recipe,
        double targetAmount,
        HashSet<string> processing,
        HashSet<string> issued,
        HqInventory inventory,
        CancellationToken cancellationToken)
    {
        if (!processing.Add(recipe.Id)) return;

        var primaryProduct = recipe.Products.FirstOrDefault(p => p.ComponentId == recipe.Id)
                             ?? recipe.Products.FirstOrDefault();
        var outputPerCraft = primaryProduct?.Amount ?? 1.0;
        var craftsNeeded = Math.Ceiling(targetAmount / outputPerCraft);

        foreach (var ingredient in recipe.Ingredients)
        {
            var subRecipe = await _catalog.GetRecipeAsync(ingredient.ComponentId, cancellationToken);
            if (subRecipe == null) continue;

            var totalNeeded = craftsNeeded * ingredient.Amount;

            if (inventory.Items.TryGetValue(ingredient.ComponentId, out var available) && available > 0)
            {
                var toConsume = Math.Min(available, totalNeeded);
                inventory.Remove(ingredient.ComponentId, toConsume);
                totalNeeded -= toConsume;
            }

            if (totalNeeded <= 0) continue;
            if (!issued.Add(subRecipe.Id)) continue;

            await IssueSubOrdersAsync(subRecipe, totalNeeded, processing, issued, inventory, cancellationToken);
            await IssueOrderAsync(subRecipe, totalNeeded, cancellationToken);
        }
    }

    private async Task<Guid> IssueOrderAsync(Recipe recipe, double targetAmount, CancellationToken cancellationToken)
    {
        var order = new ProductionOrder(recipe.Id, targetAmount);
        await _orderRepository.AddAsync(order, cancellationToken);
        order.StartProduction();
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _dispatcher.DispatchAsync(order, recipe, cancellationToken);
        return order.Id;
    }
}