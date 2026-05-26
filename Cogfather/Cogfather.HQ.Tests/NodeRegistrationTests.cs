using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Xunit;

namespace Cogfather.HQ.Tests;

public class NodeRegistrationTests
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange
        var nodeId = "node1";
        var address = "http://localhost:5000";

        // Act
        var node = new NodeRegistration(nodeId, address);

        // Assert
        Assert.Equal(nodeId, node.NodeId);
        Assert.Equal(address, node.Address);
        Assert.Equal(NodeStatus.Active, node.Status);
        Assert.Equal(FaultMode.None, node.FaultMode);
    }

    [Fact]
    public void UpdateStatus_ChangesStatus()
    {
        // Arrange
        var node = new NodeRegistration("node1", "address");

        // Act
        node.UpdateStatus(NodeStatus.Offline);

        // Assert
        Assert.Equal(NodeStatus.Offline, node.Status);
    }

    [Fact]
    public void SetFaultMode_ChangesFaultMode()
    {
        // Arrange
        var node = new NodeRegistration("node1", "address");

        // Act
        node.SetFaultMode(FaultMode.Byzantine);

        // Assert
        Assert.Equal(FaultMode.Byzantine, node.FaultMode);
    }
}