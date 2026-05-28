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

        public Task<Recipe?> GetRecipeAsync(string recipeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Recipe);
        }

        public Task<IEnumerable<Recipe>> GetAllRecipesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Recipe == null ? Enumerable.Empty<Recipe>() : new[] { Recipe });
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
        var handler = new IssueProductionOrderCommandHandler(catalog, repo, inventoryRepo, dispatcher);

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
        var handler = new IssueProductionOrderCommandHandler(catalog, repo, inventoryRepo, dispatcher);

        var command = new IssueProductionOrderCommand("recipe1", 100);

        // Act & Assert
        await Assert.ThrowsAsync<RecipeNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}