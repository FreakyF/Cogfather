using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Application.Services;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Xunit;

namespace Cogfather.HQ.Tests;

public class QuorumReportCollectorTests
{
    private class MockNodeRepository : INodeRepository
    {
        public List<NodeRegistration> Nodes { get; set; } = new();

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
    public async Task HasQuorumAsync_NoActiveNodes_ReturnsTrue()
    {
        // Arrange
        var repository = new MockNodeRepository();
        var collector = new QuorumReportCollector(repository);
        var reports = new List<ProductionReport>();

        // Act
        var result = await collector.HasQuorumAsync(reports);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(1, 0, false)]
    [InlineData(1, 1, true)]
    [InlineData(2, 1, false)]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, false)]
    [InlineData(3, 3, true)]
    [InlineData(4, 2, false)]
    [InlineData(4, 3, true)]
    [InlineData(7, 4, false)]
    [InlineData(7, 5, true)]
    [InlineData(10, 6, false)]
    [InlineData(10, 7, true)]
    public async Task HasQuorumAsync_WithActiveNodes_ReturnsExpectedResult(int activeNodesCount, int reportCount,
        bool expectedResult)
    {
        // Arrange
        var repository = new MockNodeRepository();
        for (var i = 0; i < activeNodesCount; i++)
            repository.Nodes.Add(new NodeRegistration($"node{i}", $"address{i}"));

        // Add some inactive nodes to ensure they are ignored
        repository.Nodes.Add(CreateInactiveNode("inactive1"));
        repository.Nodes.Add(CreateInactiveNode("inactive2"));

        var collector = new QuorumReportCollector(repository);
        var reports = Enumerable.Range(0, reportCount).Select(i => new ProductionReport($"node{i}", "recipe1", true))
            .ToList();

        // Act
        var result = await collector.HasQuorumAsync(reports);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    private NodeRegistration CreateInactiveNode(string id)
    {
        var node = new NodeRegistration(id, "address");
        node.UpdateStatus(NodeStatus.Offline);
        return node;
    }
}