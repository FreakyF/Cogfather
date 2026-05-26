using System;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Exceptions;
using Xunit;

namespace Cogfather.HQ.Tests;

public class HqInventoryTests
{
    [Fact]
    public void Add_NewComponent_AddsToInventory()
    {
        // Arrange
        var inventory = new HqInventory();
        var componentId = "comp1";
        var amount = 10.5;

        // Act
        inventory.Add(componentId, amount);

        // Assert
        Assert.True(inventory.Items.ContainsKey(componentId));
        Assert.Equal(amount, inventory.Items[componentId]);
    }

    [Fact]
    public void Add_ExistingComponent_IncrementsAmount()
    {
        // Arrange
        var inventory = new HqInventory();
        var componentId = "comp1";
        inventory.Add(componentId, 10);

        // Act
        inventory.Add(componentId, 5.5);

        // Assert
        Assert.Equal(15.5, inventory.Items[componentId]);
    }

    [Fact]
    public void Add_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var inventory = new HqInventory();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => inventory.Add("comp1", -1));
    }

    [Fact]
    public void Remove_SufficientInventory_DecrementsAmount()
    {
        // Arrange
        var inventory = new HqInventory();
        var componentId = "comp1";
        inventory.Add(componentId, 10);

        // Act
        inventory.Remove(componentId, 4);

        // Assert
        Assert.Equal(6, inventory.Items[componentId]);
    }

    [Fact]
    public void Remove_InsufficientInventory_ThrowsInsufficientInventoryException()
    {
        // Arrange
        var inventory = new HqInventory();
        var componentId = "comp1";
        inventory.Add(componentId, 5);

        // Act & Assert
        Assert.Throws<InsufficientInventoryException>(() => inventory.Remove(componentId, 10));
    }

    [Fact]
    public void Remove_NonExistentComponent_ThrowsInsufficientInventoryException()
    {
        // Arrange
        var inventory = new HqInventory();

        // Act & Assert
        Assert.Throws<InsufficientInventoryException>(() => inventory.Remove("none", 1));
    }

    [Fact]
    public void Remove_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var inventory = new HqInventory();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => inventory.Remove("comp1", -1));
    }
}