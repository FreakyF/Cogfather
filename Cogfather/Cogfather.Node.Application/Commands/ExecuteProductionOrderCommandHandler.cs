using Cogfather.Node.Application.Interfaces;
using Cogfather.Node.Domain.Interfaces;
using Cogfather.Node.Domain.Services;
using Cogfather.Node.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cogfather.Node.Application.Commands;

public class ExecuteProductionOrderCommandHandler : IRequestHandler<ExecuteProductionOrderCommand>
{
    private readonly ProductionExecution _executionService;
    private readonly IFaultInjector _faultInjector;
    private readonly IManifestHashService _hashService;
    private readonly IInventoryStore _inventoryStore;
    private readonly ILogger<ExecuteProductionOrderCommandHandler> _logger;
    private readonly NodeIdentity _nodeIdentity;
    private readonly IReportPublisher _reportPublisher;

    public ExecuteProductionOrderCommandHandler(
        ProductionExecution executionService,
        IInventoryStore inventoryStore,
        IReportPublisher reportPublisher,
        IManifestHashService hashService,
        IFaultInjector faultInjector,
        NodeIdentity nodeIdentity,
        ILogger<ExecuteProductionOrderCommandHandler> logger)
    {
        _executionService = executionService;
        _inventoryStore = inventoryStore;
        _reportPublisher = reportPublisher;
        _hashService = hashService;
        _faultInjector = faultInjector;
        _nodeIdentity = nodeIdentity;
        _logger = logger;
    }

    public async Task Handle(ExecuteProductionOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing production order for CorrelationId: {CorrelationId}", request.CorrelationId);

        var manifest = ProductionManifest.Create(request.CorrelationId, request.ComponentId, request.Amount);
        var inventory = _inventoryStore.GetInventory();

        var result = await _executionService.ExecuteAsync(manifest, inventory);

        if (result.ErrorMessage == "Silent Failure occurred.")
        {
            _logger.LogWarning(
                "Silent failure mode triggered. No report will be published for CorrelationId: {CorrelationId}",
                request.CorrelationId);
            return;
        }

        var hash = _hashService.GenerateHash(manifest);
        var finalHash = _faultInjector.TamperHash(hash);

        await _reportPublisher.PublishProductionReportAsync(
            request.CorrelationId,
            _nodeIdentity.NodeId,
            result.ComponentId,
            result.AmountProduced,
            result.Success,
            finalHash,
            result.ErrorMessage);

        _logger.LogInformation("Production execution completed and report published for CorrelationId: {CorrelationId}",
            request.CorrelationId);
    }
}