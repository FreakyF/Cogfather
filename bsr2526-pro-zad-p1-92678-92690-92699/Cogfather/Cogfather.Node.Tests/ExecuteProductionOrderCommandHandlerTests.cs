using System;
using System.Threading;
using System.Threading.Tasks;
using Cogfather.Node.Application.Commands;
using Cogfather.Node.Application.Interfaces;
using Cogfather.Node.Domain.Entities;
using Cogfather.Node.Domain.Enums;
using Cogfather.Node.Domain.Interfaces;
using Cogfather.Node.Domain.Records;
using Cogfather.Node.Domain.Services;
using Cogfather.Node.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Cogfather.Node.Tests;

public class ExecuteProductionOrderCommandHandlerTests
{
    private class MockInventoryStore : IInventoryStore
    {
        public NodeInventory Inventory { get; } = new();

        public NodeInventory GetInventory()
        {
            return Inventory;
        }

        public Task SaveAsync(NodeInventory inventory, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private class MockReportPublisher : IReportPublisher
    {
        public bool Called { get; private set; }

        public Task PublishProductionReportAsync(Guid correlationId, string nodeId, string componentId,
            int amountProduced, bool success, string hash, string errorMessage = null)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    private class MockHashService : IManifestHashService
    {
        public string GenerateHash(ProductionManifest manifest)
        {
            return "hash";
        }
    }

    private class MockFaultInjector : IFaultInjector
    {
        public virtual FaultMode CurrentMode => FaultMode.None;

        public virtual void SetFaultMode(FaultMode mode)
        {
        }

        public virtual Task ApplyDelayIfNeededAsync()
        {
            return Task.CompletedTask;
        }

        public virtual bool ShouldSilentlyFail()
        {
            return false;
        }

        public virtual ProductionManifest ManipulateManifest(ProductionManifest original)
        {
            return original;
        }

        public virtual ProductionResult ManipulateResult(ProductionResult original)
        {
            return original;
        }

        public virtual int GetLieInventoryCount(string componentId, int actualCount)
        {
            return actualCount;
        }

        public virtual string TamperHash(string actualHash)
        {
            return actualHash;
        }
    }

    private class MockSystemLogPublisher : ISystemLogPublisher
    {
        public Task PublishAsync(string level, string category, string message, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private class MockLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
        }
    }

    [Fact]
    public async Task Handle_SuccessfulExecution_PublishesReport()
    {
        // Arrange
        var faultInjector = new MockFaultInjector();
        var executionService = new ProductionExecution(faultInjector);
        var store = new MockInventoryStore();
        var publisher = new MockReportPublisher();
        var hashService = new MockHashService();
        var identity = new NodeIdentity("node1");
        var logger = new MockLogger<ExecuteProductionOrderCommandHandler>();

        var handler = new ExecuteProductionOrderCommandHandler(executionService, store, publisher, hashService,
            faultInjector, identity, new MockSystemLogPublisher(), logger);
        var command = new ExecuteProductionOrderCommand(Guid.NewGuid(), "comp1", 10);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(publisher.Called);
    }

    private class SilentFailFaultInjector : MockFaultInjector
    {
        public override bool ShouldSilentlyFail()
        {
            return true;
        }
    }

    [Fact]
    public async Task Handle_SilentFailure_DoesNotPublishReport()
    {
        // Arrange
        var faultInjector = new SilentFailFaultInjector();
        var executionService = new ProductionExecution(faultInjector);
        var store = new MockInventoryStore();
        var publisher = new MockReportPublisher();
        var hashService = new MockHashService();
        var identity = new NodeIdentity("node1");
        var logger = new MockLogger<ExecuteProductionOrderCommandHandler>();

        var handler = new ExecuteProductionOrderCommandHandler(executionService, store, publisher, hashService,
            faultInjector, identity, new MockSystemLogPublisher(), logger);
        var command = new ExecuteProductionOrderCommand(Guid.NewGuid(), "comp1", 10);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(publisher.Called);
    }
}