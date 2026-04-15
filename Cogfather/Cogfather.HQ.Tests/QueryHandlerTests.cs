using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Application.Queries.GetAllNodes;
using Cogfather.HQ.Application.Queries.GetInventory;
using Cogfather.HQ.Application.Queries.GetOrderById;
using Cogfather.HQ.Application.Queries.GetOrders;
using Cogfather.HQ.Domain.Entities;
using Xunit;

namespace Cogfather.HQ.Tests;

public class QueryHandlerTests
{
    private class MockNodeRepository : INodeRepository
    {
        public List<NodeRegistration> Nodes { get; } = new();

        public Task<NodeRegistration?> GetByIdAsync(string nodeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Nodes.FirstOrDefault(n => n.NodeId == nodeId));
        }

        public Task<IEnumerable<NodeRegistration>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<NodeRegistration>>(Nodes);
        }

        public Task AddAsync(NodeRegistration node, CancellationToken cancellationToken = default)
        {
            Nodes.Add(node);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(NodeRegistration node, CancellationToken cancellationToken = default)
        {
            var existing = Nodes.FirstOrDefault(n => n.NodeId == node.NodeId);
            if (existing != null)
            {
                Nodes.Remove(existing);
                Nodes.Add(node);
            }

            return Task.CompletedTask;
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
            var existing = Orders.FirstOrDefault(o => o.Id == order.Id);
            if (existing != null)
            {
                Orders.Remove(existing);
                Orders.Add(order);
            }

            return Task.CompletedTask;
        }

        public Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Orders.FirstOrDefault(o => o.Id == id));
        }

        public Task<IEnumerable<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<ProductionOrder>>(Orders);
        }
    }

    private class MockInventoryRepository : IInventoryRepository
    {
        public HqInventory Inventory { get; } = new();

        public Task<HqInventory> GetAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Inventory);
        }

        public Task SaveAsync(HqInventory inventory, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task GetAllNodesQueryHandler_ReturnsAllNodes()
    {
        // Arrange
        var repo = new MockNodeRepository();
        repo.Nodes.Add(new NodeRegistration("node1", "addr1"));
        var handler = new GetAllNodesQueryHandler(repo);

        // Act
        var result = await handler.Handle(new GetAllNodesQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("node1", result.First().NodeId);
    }

    [Fact]
    public async Task GetOrdersQueryHandler_ReturnsAllOrders()
    {
        // Arrange
        var repo = new MockOrderRepository();
        repo.Orders.Add(new ProductionOrder("recipe1", 100));
        var handler = new GetOrdersQueryHandler(repo);

        // Act
        var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetOrderByIdQueryHandler_ReturnsOrder()
    {
        // Arrange
        var repo = new MockOrderRepository();
        var order = new ProductionOrder("recipe1", 100);
        repo.Orders.Add(order);
        var handler = new GetOrderByIdQueryHandler(repo);

        // Act
        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
    }

    [Fact]
    public async Task GetInventoryQueryHandler_ReturnsInventory()
    {
        // Arrange
        var repo = new MockInventoryRepository();
        repo.Inventory.Add("comp1", 10);
        var handler = new GetInventoryQueryHandler(repo);

        // Act
        var result = await handler.Handle(new GetInventoryQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(10, result.Items["comp1"]);
    }
}