using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cogfather.HQ.Application.Commands.RegisterNode;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Xunit;

namespace Cogfather.HQ.Tests;

public class RegisterNodeCommandHandlerTests
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
    public async Task Handle_NewNode_AddsToRepository()
    {
        // Arrange
        var repo = new MockNodeRepository();
        var handler = new RegisterNodeCommandHandler(repo);
        var command = new RegisterNodeCommand("node1", "http://addr");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(repo.Nodes);
        Assert.Equal("node1", repo.Nodes[0].NodeId);
    }

    [Fact]
    public async Task Handle_ExistingNode_DoesNotAdd()
    {
        // Arrange
        var repo = new MockNodeRepository();
        repo.Nodes.Add(new NodeRegistration("node1", "old"));
        var handler = new RegisterNodeCommandHandler(repo);
        var command = new RegisterNodeCommand("node1", "new");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(repo.Nodes);
        Assert.Equal("old", repo.Nodes[0].Address);
    }
}