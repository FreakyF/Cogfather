using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Application.Metrics;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Exceptions;
using Cogfather.HQ.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cogfather.HQ.Application.Commands.IssueProductionOrder;

public class IssueProductionOrderCommandHandler : IRequestHandler<IssueProductionOrderCommand, Guid>
{
    private readonly IProductionCatalog _catalog;
    private readonly IOrderDispatcher _dispatcher;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<IssueProductionOrderCommandHandler> _logger;
    private readonly IProductionOrderRepository _orderRepository;

    public IssueProductionOrderCommandHandler(
        IProductionCatalog catalog,
        IProductionOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IOrderDispatcher dispatcher,
        ILogger<IssueProductionOrderCommandHandler> logger)
    {
        _catalog = catalog;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task<Guid> Handle(IssueProductionOrderCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _catalog.GetRecipeAsync(request.RecipeId, cancellationToken)
                     ?? throw new RecipeNotFoundException(request.RecipeId);

        _logger.LogInformation("Issuing order: {RecipeId} × {Amount}", request.RecipeId, request.TargetAmount);
        CogfatherMetrics.OrdersIssued.WithLabels(request.RecipeId).Inc();

        var inventory = await _inventoryRepository.GetAsync(cancellationToken);

        var pending = new Dictionary<string, double>();
        await CollectSubOrdersAsync(recipe, request.TargetAmount, new HashSet<string>(), pending, inventory, cancellationToken);

        await _inventoryRepository.SaveAsync(inventory, cancellationToken);

        foreach (var (subRecipeId, amount) in pending)
        {
            var subRecipe = await _catalog.GetRecipeAsync(subRecipeId, cancellationToken);
            if (subRecipe != null)
            {
                _logger.LogInformation("Sub-order: {RecipeId} × {Amount}", subRecipeId, amount);
                await IssueOrderAsync(subRecipe, amount, cancellationToken);
            }
        }

        var orderId = await IssueOrderAsync(recipe, request.TargetAmount, cancellationToken);
        _logger.LogInformation("Order dispatched: {OrderId} ({RecipeId} × {Amount})", orderId, request.RecipeId, request.TargetAmount);
        return orderId;
    }

    private async Task CollectSubOrdersAsync(
        Recipe recipe,
        double targetAmount,
        HashSet<string> processing,
        Dictionary<string, double> pending,
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

            pending[subRecipe.Id] = pending.GetValueOrDefault(subRecipe.Id) + totalNeeded;

            await CollectSubOrdersAsync(subRecipe, totalNeeded, processing, pending, inventory, cancellationToken);
        }

        processing.Remove(recipe.Id);
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