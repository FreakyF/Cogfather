using System;
using System.Threading.Tasks;
using Cogfather.Node.Application.Configuration;
using Cogfather.Node.Application.Services;
using Cogfather.Node.Domain.Enums;
using Cogfather.Node.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Xunit;

namespace Cogfather.Node.Tests;

public class FaultInjectorTests
{
    private IOptions<FaultConfiguration> CreateConfig()
    {
        return Options.Create(new FaultConfiguration
        {
            DelayMilliseconds = 10,
            SilentFailureProbability = 1.0, // Always fail for testing
            ManipulatedAmountOffset = 5,
            ManipulatedComponentIdSuffix = "_bad",
            InventoryLieOffset = 100
        });
    }

    [Fact]
    public void SetFaultMode_UpdatesCurrentMode()
    {
        // Arrange
        var injector = new FaultInjector(CreateConfig());

        // Act
        injector.SetFaultMode(FaultMode.DelayedResponse);

        // Assert
        Assert.Equal(FaultMode.DelayedResponse, injector.CurrentMode);
    }

    [Fact]
    public async Task ApplyDelayIfNeededAsync_DelayedResponse_Waits()
    {
        // Arrange
        var injector = new FaultInjector(CreateConfig());
        injector.SetFaultMode(FaultMode.DelayedResponse);

        // Act & Assert
        await injector.ApplyDelayIfNeededAsync(); // Should just complete
    }

    [Fact]
    public void ShouldSilentlyFail_SilentFailure_ReturnsTrueWhenProbabilityIs1()
    {
        // Arrange
        var injector = new FaultInjector(CreateConfig());
        injector.SetFaultMode(FaultMode.SilentFailure);

        // Act
        var result = injector.ShouldSilentlyFail();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ManipulateManifest_DataManipulation_AltersManifest()
    {
        // Arrange
        var injector = new FaultInjector(CreateConfig());
        injector.SetFaultMode(FaultMode.DataManipulation);
        var original = ProductionManifest.Create(Guid.NewGuid(), "comp1", 10);

        // Act
        var result = injector.ManipulateManifest(original);

        // Assert
        Assert.Equal(15, result.Amount);
        Assert.Equal("comp1_bad", result.ComponentId);
    }

    [Fact]
    public void GetLieInventoryCount_InventoryLie_ReturnsManipulatedCount()
    {
        // Arrange
        var injector = new FaultInjector(CreateConfig());
        injector.SetFaultMode(FaultMode.InventoryLie);

        // Act
        var result = injector.GetLieInventoryCount("comp1", 50);

        // Assert
        Assert.Equal(150, result);
    }

    [Fact]
    public void TamperHash_HashTampering_ReturnsDeadBeefHash()
    {
        // Arrange
        var injector = new FaultInjector(CreateConfig());
        injector.SetFaultMode(FaultMode.HashTampering);

        // Act
        var result = injector.TamperHash("0123456789ABCDEF");

        // Assert
        Assert.StartsWith("DEADBEEF", result);
        Assert.Equal("DEADBEEF89ABCDEF", result);
    }

    [Fact]
    public void TamperHash_HashTampering_ShortHash_ReturnsDeadBeef()
    {
        // Arrange
        var injector = new FaultInjector(CreateConfig());
        injector.SetFaultMode(FaultMode.HashTampering);

        // Act
        var result = injector.TamperHash("ABC");

        // Assert
        Assert.Equal("DEADBEEF", result);
    }
}