using System.Threading.Tasks;
using Cogfather.Node.Domain.Entities;
using Cogfather.Node.Domain.Enums;
using Cogfather.Node.Domain.Interfaces;
using Cogfather.Node.Domain.Records;
using Cogfather.Node.Domain.Services;
using Cogfather.Node.Domain.ValueObjects;

namespace Cogfather.Node.Tests;

public class ProductionExecutionTests
{
    private class MockFaultInjector : IFaultInjector
    {
        public FaultMode CurrentMode { get; private set; }
        public Func<ProductionManifest, ProductionManifest> ManifestManipulator { get; set; } = m => m;
        public Func<ProductionResult, ProductionResult> ResultManipulator { get; set; } = r => r;
        public bool SilentFail { get; set; }
        public bool ApplyDelayCalled { get; private set; }

        public void SetFaultMode(FaultMode mode)
        {
            CurrentMode = mode;
        }

        public Task ApplyDelayIfNeededAsync()
        {
            ApplyDelayCalled = true;
            return Task.CompletedTask;
        }

        public bool ShouldSilentlyFail()
        {
            return SilentFail;
        }

        public ProductionManifest ManipulateManifest(ProductionManifest original)
        {
            return ManifestManipulator(original);
        }

        public ProductionResult ManipulateResult(ProductionResult original)
        {
            return ResultManipulator(original);
        }

        public int GetLieInventoryCount(string componentId, int actualCount)
        {
            return actualCount;
        }

        public string TamperHash(string actualHash)
        {
            return actualHash;
        }
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulExecution_AddsToInventoryAndReturnsSuccess()
    {
        // Arrange
        var faultInjector = new MockFaultInjector();
        var executionService = new ProductionExecution(faultInjector);
        var inventory = new NodeInventory();
        var manifest = ProductionManifest.Create(Guid.NewGuid(), "comp1", 10);

        // Act
        var result = await executionService.ExecuteAsync(manifest, inventory);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10, inventory.GetCount("comp1"));
        Assert.Equal("comp1", result.ComponentId);
        Assert.Equal(10, result.AmountProduced);
        Assert.True(faultInjector.ApplyDelayCalled);
    }

    [Fact]
    public async Task ExecuteAsync_SilentFail_ReturnsFailureAndDoesNotAddToInventory()
    {
        // Arrange
        var faultInjector = new MockFaultInjector { SilentFail = true };
        var executionService = new ProductionExecution(faultInjector);
        var inventory = new NodeInventory();
        var manifest = ProductionManifest.Create(Guid.NewGuid(), "comp1", 10);

        // Act
        var result = await executionService.ExecuteAsync(manifest, inventory);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Silent Failure occurred.", result.ErrorMessage);
        Assert.Equal(0, inventory.GetCount("comp1"));
        Assert.True(faultInjector.ApplyDelayCalled);
    }

    [Fact]
    public async Task ExecuteAsync_ManifestManipulation_AddsManipulatedComponentToInventory()
    {
        // Arrange
        var faultInjector = new MockFaultInjector
        {
            ManifestManipulator = m => ProductionManifest.Create(m.CorrelationId, "comp2", 20)
        };
        var executionService = new ProductionExecution(faultInjector);
        var inventory = new NodeInventory();
        var manifest = ProductionManifest.Create(Guid.NewGuid(), "comp1", 10);

        // Act
        var result = await executionService.ExecuteAsync(manifest, inventory);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, inventory.GetCount("comp1"));
        Assert.Equal(20, inventory.GetCount("comp2"));
        Assert.Equal("comp2", result.ComponentId);
        Assert.Equal(20, result.AmountProduced);
    }

    [Fact]
    public async Task ExecuteAsync_ResultManipulation_ReturnsManipulatedResult()
    {
        // Arrange
        var faultInjector = new MockFaultInjector
        {
            ResultManipulator = r => ProductionResult.Failed(r.ComponentId, r.AmountProduced, "manipulated error")
        };
        var executionService = new ProductionExecution(faultInjector);
        var inventory = new NodeInventory();
        var manifest = ProductionManifest.Create(Guid.NewGuid(), "comp1", 10);

        // Act
        var result = await executionService.ExecuteAsync(manifest, inventory);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("manipulated error", result.ErrorMessage);
        Assert.Equal(10, inventory.GetCount("comp1"));
    }
}