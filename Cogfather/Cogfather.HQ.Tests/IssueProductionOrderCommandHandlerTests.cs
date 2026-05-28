using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cogfather.HQ.Application.Commands.IssueProductionOrder;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Cogfather.HQ.Domain.Exceptions;
using Cogfather.HQ.Domain.ValueObjects;
using Xunit;

namespace Cogfather.HQ.Tests;

public class IssueProductionOrderCommandHandlerTests
{
    private class MockCatalog : IProductionCatalog
    {
        public Recipe? Recipe { get; set; }
        public Dictionary<string, Recipe> Recipes { get; } = new();

        public Task<Recipe?> GetRecipeAsync(string recipeId, CancellationToken cancellationToken = default)
        {
            if (Recipes.TryGetValue(recipeId, out var r)) return Task.FromResult<Recipe?>(r);
            return Task.FromResult(Recipe?.Id == recipeId ? Recipe : null);
        }

        public Task<IEnumerable<Recipe>> GetAllRecipesAsync(CancellationToken cancellationToken = default)
        {
            var all = Recipes.Values.ToList();
            if (Recipe != null) all.Add(Recipe);
            return Task.FromResult<IEnumerable<Recipe>>(all);
        }
    }

    private class MockOrderRepository : IProductionOrderRepository
    {
        public List<ProductionOrder> Orders { get; } = new();

        public Task AddAsync(ProductionOrder order, CancellationToken cancellationToken = default)
        {
            Orders.Add(order);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ProductionOrder order, CancellationToken cancellationToken = default)
        {
            var existing = Orders.Find(o => o.Id == order.Id);
            if (existing != null)
            {
                Orders.Remove(existing);
                Orders.Add(order);
            }

            return Task.CompletedTask;
        }

        public Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Orders.Find(o => o.Id == id));
        }

        public Task<IEnumerable<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<ProductionOrder>>(Orders);
        }
    }

    private class MockOrderDispatcher : IOrderDispatcher
    {
        public bool Called { get; private set; }

        public Task DispatchAsync(ProductionOrder order, Recipe recipe, CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    private class MockInventoryRepository : IInventoryRepository
    {
        private readonly HqInventory _inventory;

        public MockInventoryRepository(HqInventory? inventory = null)
        {
            _inventory = inventory ?? new HqInventory();
        }

        public Task<HqInventory> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_inventory);

        public Task SaveAsync(HqInventory inventory, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddItemAsync(string componentId, double amount, CancellationToken cancellationToken = default)
        {
            _inventory.Add(componentId, amount);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_RecipeExists_AddsOrderAndDispatches()
    {
        // Arrange
        var catalog = new MockCatalog
        { Recipe = new Recipe("recipe1", 10.0, new List<Ingredient>(), new List<Product>()) };
        var repo = new MockOrderRepository();
        var dispatcher = new MockOrderDispatcher();
        var inventoryRepo = new MockInventoryRepository();
        var handler = new IssueProductionOrderCommandHandler(catalog, repo, inventoryRepo, dispatcher, Microsoft.Extensions.Logging.Abstractions.NullLogger<IssueProductionOrderCommandHandler>.Instance);

        var command = new IssueProductionOrderCommand("recipe1", 100);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.Single(repo.Orders);
        Assert.Equal(ProductionOrderStatus.InProgress, repo.Orders[0].Status);
        Assert.True(dispatcher.Called);
    }

    [Fact]
    public async Task Handle_RecipeNotFound_ThrowsRecipeNotFoundException()
    {
        // Arrange
        var catalog = new MockCatalog { Recipe = null };
        var repo = new MockOrderRepository();
        var dispatcher = new MockOrderDispatcher();
        var inventoryRepo = new MockInventoryRepository();
        var handler = new IssueProductionOrderCommandHandler(catalog, repo, inventoryRepo, dispatcher, Microsoft.Extensions.Logging.Abstractions.NullLogger<IssueProductionOrderCommandHandler>.Instance);

        var command = new IssueProductionOrderCommand("recipe1", 100);

        // Act & Assert
        await Assert.ThrowsAsync<RecipeNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RecipeWithIngredients_IssuesSubOrders()
    {
        // copper-cable recipe (ingredient for wire)
        var cableRecipe = new Recipe("copper-cable", 1.0,
            [],
            [new Product("copper-cable", 2)]);

        // wire recipe needs copper-cable
        var wireRecipe = new Recipe("wire", 1.0,
            [new Ingredient("copper-cable", 1)],
            [new Product("wire", 1)]);

        var catalog = new MockCatalog();
        catalog.Recipes["wire"] = wireRecipe;
        catalog.Recipes["copper-cable"] = cableRecipe;

        var repo = new MockOrderRepository();
        var dispatcher = new MockOrderDispatcher();
        var inventoryRepo = new MockInventoryRepository();
        var handler = new IssueProductionOrderCommandHandler(catalog, repo, inventoryRepo, dispatcher,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<IssueProductionOrderCommandHandler>.Instance);

        await handler.Handle(new IssueProductionOrderCommand("wire", 2), CancellationToken.None);

        // Expect 2 orders: one for copper-cable sub-order, one for wire
        Assert.Equal(2, repo.Orders.Count);
        Assert.Contains(repo.Orders, o => o.RecipeId == "copper-cable");
        Assert.Contains(repo.Orders, o => o.RecipeId == "wire");
    }

    [Fact]
    public async Task Handle_DiamondDependency_AccumulatesAmountsForSharedSubComponent()
    {
        // gadget needs part-a (×1) and part-b (×1)
        // part-a needs shared-component (×2)
        // part-b needs shared-component (×1)
        // → shared-component sub-order must be for 3 total, not 2 (bug: second branch skipped)
        var sharedRecipe = new Recipe("shared-component", 1.0, [], [new Product("shared-component", 1)]);
        var partA = new Recipe("part-a", 1.0,
            [new Ingredient("shared-component", 2)],
            [new Product("part-a", 1)]);
        var partB = new Recipe("part-b", 1.0,
            [new Ingredient("shared-component", 1)],
            [new Product("part-b", 1)]);
        var gadget = new Recipe("gadget", 1.0,
            [new Ingredient("part-a", 1), new Ingredient("part-b", 1)],
            [new Product("gadget", 1)]);

        var catalog = new MockCatalog();
        catalog.Recipes["gadget"] = gadget;
        catalog.Recipes["part-a"] = partA;
        catalog.Recipes["part-b"] = partB;
        catalog.Recipes["shared-component"] = sharedRecipe;

        var repo = new MockOrderRepository();
        var dispatcher = new MockOrderDispatcher();
        var inventoryRepo = new MockInventoryRepository();
        var handler = new IssueProductionOrderCommandHandler(catalog, repo, inventoryRepo, dispatcher,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<IssueProductionOrderCommandHandler>.Instance);

        await handler.Handle(new IssueProductionOrderCommand("gadget", 1), CancellationToken.None);

        // Expect 4 orders: shared-component, part-a, part-b, gadget
        Assert.Equal(4, repo.Orders.Count);
        var sharedOrder = repo.Orders.Single(o => o.RecipeId == "shared-component");
        Assert.Equal(3, sharedOrder.TargetAmount); // 2 from part-a + 1 from part-b
    }

    [Fact]
    public async Task Handle_IngredientAlreadyInInventory_ConsumesInventoryBeforeIssuingSubOrder()
    {
        var cableRecipe = new Recipe("copper-cable", 1.0,
            [],
            [new Product("copper-cable", 2)]);

        var wireRecipe = new Recipe("wire", 1.0,
            [new Ingredient("copper-cable", 2)],
            [new Product("wire", 1)]);

        var catalog = new MockCatalog();
        catalog.Recipes["wire"] = wireRecipe;
        catalog.Recipes["copper-cable"] = cableRecipe;

        var inventory = new HqInventory();
        inventory.Add("copper-cable", 10); // enough to satisfy demand
        var inventoryRepo = new MockInventoryRepository(inventory);

        var repo = new MockOrderRepository();
        var dispatcher = new MockOrderDispatcher();
        var handler = new IssueProductionOrderCommandHandler(catalog, repo, inventoryRepo, dispatcher,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<IssueProductionOrderCommandHandler>.Instance);

        await handler.Handle(new IssueProductionOrderCommand("wire", 1), CancellationToken.None);

        // Only main order, no sub-order needed since inventory covered the ingredient
        Assert.Single(repo.Orders);
        Assert.Equal("wire", repo.Orders[0].RecipeId);
    }
}