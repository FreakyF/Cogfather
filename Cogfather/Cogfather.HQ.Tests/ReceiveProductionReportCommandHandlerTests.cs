using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cogfather.HQ.Application.Commands.ReceiveProductionReport;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Application.Services;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Cogfather.HQ.Domain.Interfaces;
using Cogfather.HQ.Domain.ValueObjects;
using Xunit;

namespace Cogfather.HQ.Tests;

public class ReceiveProductionReportCommandHandlerTests
{
    private class MockReportRepository : IProductionReportRepository
    {
        public List<ProductionReport> Reports { get; } = new();

        public Task AddAsync(ProductionReport report, CancellationToken cancellationToken = default)
        {
            Reports.Add(report);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ProductionReport>> GetByCorrelationIdAsync(Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Reports.Where(r => r.CorrelationId == correlationId));
        }

        public Task<IEnumerable<ProductionReport>> GetByRecipeIdAsync(string recipeId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Reports.Where(r => r.RecipeId == recipeId));
        }

        public Task<IEnumerable<ProductionReport>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<ProductionReport>>(Reports);
        }
    }

    private class MockOrderRepository : IProductionOrderRepository
    {
        public Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<ProductionOrder?>(null);

        public Task<IEnumerable<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<ProductionOrder>>([]);

        public Task AddAsync(ProductionOrder order, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(ProductionOrder order, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private class MockConsensusEngine : IConsensusEngine
    {
        public ConsensusResult Result { get; set; } = new("test", ConsensusVerdict.Approved, 1.0);
        public bool Called { get; private set; }

        public Task<ConsensusResult> EvaluateAsync(string recipeId, IEnumerable<ProductionReport> reports)
        {
            Called = true;
            return Task.FromResult(Result);
        }
    }

    private class MockConsensusNotifier : IConsensusNotifier
    {
        public bool Called { get; private set; }

        public Task NotifyAsync(ConsensusResult result, CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

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

    [Fact]
    public async Task Handle_AddsReportAndProceedsToConsensusWhenQuorumMet()
    {
        // Arrange
        var reportRepo = new MockReportRepository();
        var engine = new MockConsensusEngine();
        var notifier = new MockConsensusNotifier();
        var nodeRepo = new MockNodeRepository();
        nodeRepo.Nodes.Add(new NodeRegistration("node1", "addr"));

        var quorumCollector = new QuorumReportCollector(new FakeScopeFactory(nodeRepo));
        var handler = new ReceiveProductionReportCommandHandler(reportRepo, new MockOrderRepository(), engine, notifier, quorumCollector);

        var orderId = Guid.NewGuid();
        var command = new ReceiveProductionReportCommand(orderId, "node1", "recipe1", true);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(reportRepo.Reports);
        Assert.True(engine.Called);
        Assert.True(notifier.Called);
    }

    [Fact]
    public async Task Handle_DoesNotProceedToConsensusWhenQuorumNotMet()
    {
        // Arrange
        var reportRepo = new MockReportRepository();
        var engine = new MockConsensusEngine();
        var notifier = new MockConsensusNotifier();
        var nodeRepo = new MockNodeRepository();
        nodeRepo.Nodes.Add(new NodeRegistration("node1", "addr"));
        nodeRepo.Nodes.Add(new NodeRegistration("node2", "addr"));
        nodeRepo.Nodes.Add(new NodeRegistration("node3", "addr"));
        nodeRepo.Nodes.Add(new NodeRegistration("node4", "addr"));
        // n=4, f=1, 2f+1=3 required.

        var quorumCollector = new QuorumReportCollector(new FakeScopeFactory(nodeRepo));
        var handler = new ReceiveProductionReportCommandHandler(reportRepo, new MockOrderRepository(), engine, notifier, quorumCollector);

        var command = new ReceiveProductionReportCommand(Guid.NewGuid(), "node1", "recipe1", true);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(reportRepo.Reports);
        Assert.False(engine.Called);
        Assert.False(notifier.Called);
    }
}