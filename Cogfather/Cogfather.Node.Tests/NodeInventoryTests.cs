using Cogfather.Node.Domain.Entities;

namespace Cogfather.Node.Tests;

public class NodeInventoryTests
{
    [Fact]
    public void AddComponent_NewComponent_AddsToInventory()
    {
        // Arrange
        var inventory = new NodeInventory();
        var componentId = "test_component";
        var amount = 10;

        // Act
        inventory.AddComponent(componentId, amount);

        // Assert
        Assert.Equal(amount, inventory.GetCount(componentId));
    }

    [Fact]
    public void AddComponent_ExistingComponent_UpdatesAmount()
    {
        // Arrange
        var inventory = new NodeInventory();
        var componentId = "test_component";
        inventory.AddComponent(componentId, 5);

        // Act
        inventory.AddComponent(componentId, 10);

        // Assert
        Assert.Equal(15, inventory.GetCount(componentId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddComponent_InvalidComponentId_ThrowsArgumentException(string? componentId)
    {
        // Arrange
        var inventory = new NodeInventory();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => inventory.AddComponent(componentId, 10));
    }

    [Fact]
    public void AddComponent_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var inventory = new NodeInventory();
        var componentId = "test_component";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.AddComponent(componentId, -5));
    }

    [Fact]
    public void TryTakeComponent_SufficientAmount_ReturnsTrueAndUpdatesAmount()
    {
        // Arrange
        var inventory = new NodeInventory();
        var componentId = "test_component";
        inventory.AddComponent(componentId, 20);

        // Act
        var result = inventory.TryTakeComponent(componentId, 5);

        // Assert
        Assert.True(result);
        Assert.Equal(15, inventory.GetCount(componentId));
    }

    [Fact]
    public void TryTakeComponent_InsufficientAmount_ReturnsFalse()
    {
        // Arrange
        var inventory = new NodeInventory();
        var componentId = "test_component";
        inventory.AddComponent(componentId, 5);

        // Act
        var result = inventory.TryTakeComponent(componentId, 10);

        // Assert
        Assert.False(result);
        Assert.Equal(5, inventory.GetCount(componentId));
    }

    [Fact]
    public void TryTakeComponent_NonExistentComponent_ReturnsFalse()
    {
        // Arrange
        var inventory = new NodeInventory();
        var componentId = "test_component";

        // Act
        var result = inventory.TryTakeComponent(componentId, 10);

        // Assert
        Assert.False(result);
        Assert.Equal(0, inventory.GetCount(componentId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void TryTakeComponent_InvalidComponentId_ThrowsArgumentException(string? componentId)
    {
        // Arrange
        var inventory = new NodeInventory();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => inventory.TryTakeComponent(componentId, 10));
    }

    [Fact]
    public void TryTakeComponent_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var inventory = new NodeInventory();
        var componentId = "test_component";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.TryTakeComponent(componentId, -5));
    }

    [Fact]
    public void GetCount_ExistingComponent_ReturnsCorrectCount()
    {
        // Arrange
        var inventory = new NodeInventory();
        var componentId = "test_component";
        inventory.AddComponent(componentId, 7);

        // Act
        var count = inventory.GetCount(componentId);

        // Assert
        Assert.Equal(7, count);
    }

    [Fact]
    public void GetCount_NonExistentComponent_ReturnsZero()
    {
        // Arrange
        var inventory = new NodeInventory();
        var componentId = "non_existent_component";

        // Act
        var count = inventory.GetCount(componentId);

        // Assert
        Assert.Equal(0, count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetCount_InvalidComponentId_ThrowsArgumentException(string? componentId)
    {
        // Arrange
        var inventory = new NodeInventory();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => inventory.GetCount(componentId));
    }

    [Fact]
    public void GetAll_WhenCalled_ReturnsAllItems()
    {
        // Arrange
        var inventory = new NodeInventory();
        inventory.AddComponent("comp1", 10);
        inventory.AddComponent("comp2", 20);

        // Act
        var allItems = inventory.GetAll();

        // Assert
        Assert.Equal(2, allItems.Count);
        Assert.Equal(10, allItems["comp1"]);
        Assert.Equal(20, allItems["comp2"]);
    }
}