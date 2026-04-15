using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cogfather.HQ.Application.Commands.ClearNodeFault;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Xunit;

namespace Cogfather.HQ.Tests;

public class ClearNodeFaultCommandHandlerTests
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
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_NodeExists_ClearsFaultMode()
    {
        // Arrange
        var repo = new MockNodeRepository();
        var node = new NodeRegistration("node1", "addr");
        node.SetFaultMode(FaultMode.Byzantine);
        repo.Nodes.Add(node);
        var handler = new ClearNodeFaultCommandHandler(repo);
        var command = new ClearNodeFaultCommand("node1");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(FaultMode.None, node.FaultMode);
    }

    [Fact]
    public async Task Handle_NodeNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = new MockNodeRepository();
        var handler = new ClearNodeFaultCommandHandler(repo);
        var command = new ClearNodeFaultCommand("node1");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }
}