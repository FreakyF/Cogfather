using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;

namespace Cogfather.HQ.Tests;

public class ProductionOrderTests
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange
        var recipeId = "test_recipe";
        var targetAmount = 100.5;

        // Act
        var order = new ProductionOrder(recipeId, targetAmount);

        // Assert
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(recipeId, order.RecipeId);
        Assert.Equal(targetAmount, order.TargetAmount);
        Assert.Equal(ProductionOrderStatus.Pending, order.Status);
        Assert.True(order.CreatedAt <= DateTime.UtcNow && order.CreatedAt > DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void StartProduction_FromPending_ChangesStatusToInProgress()
    {
        // Arrange
        var order = new ProductionOrder("test", 10);

        // Act
        order.StartProduction();

        // Assert
        Assert.Equal(ProductionOrderStatus.InProgress, order.Status);
    }

    [Theory]
    [InlineData(ProductionOrderStatus.InProgress)]
    [InlineData(ProductionOrderStatus.Completed)]
    [InlineData(ProductionOrderStatus.Failed)]
    [InlineData(ProductionOrderStatus.Cancelled)]
    public void StartProduction_FromNonPendingStatus_ThrowsInvalidOperationException(ProductionOrderStatus initialStatus)
    {
        // Arrange
        var order = new ProductionOrder("test", 10);
        // Use reflection to set initial status for testing invalid transitions
        order.GetType().GetProperty(nameof(ProductionOrder.Status))!.SetValue(order, initialStatus);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.StartProduction());
    }

    [Fact]
    public void CompleteProduction_FromInProgress_ChangesStatusToCompleted()
    {
        // Arrange
        var order = new ProductionOrder("test", 10);
        order.StartProduction();

        // Act
        order.CompleteProduction();

        // Assert
        Assert.Equal(ProductionOrderStatus.Completed, order.Status);
    }

    [Theory]
    [InlineData(ProductionOrderStatus.Pending)]
    [InlineData(ProductionOrderStatus.Completed)]
    [InlineData(ProductionOrderStatus.Failed)]
    [InlineData(ProductionOrderStatus.Cancelled)]
    public void CompleteProduction_FromNonInProgressStatus_ThrowsInvalidOperationException(ProductionOrderStatus initialStatus)
    {
        // Arrange
        var order = new ProductionOrder("test", 10);
        order.GetType().GetProperty(nameof(ProductionOrder.Status))!.SetValue(order, initialStatus);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.CompleteProduction());
    }

    [Theory]
    [InlineData(ProductionOrderStatus.Pending)]
    [InlineData(ProductionOrderStatus.InProgress)]
    [InlineData(ProductionOrderStatus.Completed)]
    [InlineData(ProductionOrderStatus.Failed)]
    [InlineData(ProductionOrderStatus.Cancelled)]
    public void FailProduction_ChangesStatusToFailed(ProductionOrderStatus initialStatus)
    {
        // Arrange
        var order = new ProductionOrder("test", 10);
        order.GetType().GetProperty(nameof(ProductionOrder.Status))!.SetValue(order, initialStatus);

        // Act
        order.FailProduction();

        // Assert
        Assert.Equal(ProductionOrderStatus.Failed, order.Status);
    }

    [Theory]
    [InlineData(ProductionOrderStatus.Pending)]
    [InlineData(ProductionOrderStatus.InProgress)]
    public void CancelProduction_FromCancellableStatus_ChangesStatusToCancelled(ProductionOrderStatus initialStatus)
    {
        // Arrange
        var order = new ProductionOrder("test", 10);
        order.GetType().GetProperty(nameof(ProductionOrder.Status))!.SetValue(order, initialStatus);

        // Act
        order.CancelProduction();

        // Assert
        Assert.Equal(ProductionOrderStatus.Cancelled, order.Status);
    }

    [Theory]
    [InlineData(ProductionOrderStatus.Completed)]
    [InlineData(ProductionOrderStatus.Failed)]
    public void CancelProduction_FromNonCancellableStatus_ThrowsInvalidOperationException(ProductionOrderStatus initialStatus)
    {
        // Arrange
        var order = new ProductionOrder("test", 10);
        order.GetType().GetProperty(nameof(ProductionOrder.Status))!.SetValue(order, initialStatus);
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.CancelProduction());
    }
}