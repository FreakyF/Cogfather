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
    private readonly ISystemLogPublisher _systemLog;

    public ExecuteProductionOrderCommandHandler(
        ProductionExecution executionService,
        IInventoryStore inventoryStore,
        IReportPublisher reportPublisher,
        IManifestHashService hashService,
        IFaultInjector faultInjector,
        NodeIdentity nodeIdentity,
        ISystemLogPublisher systemLog,
        ILogger<ExecuteProductionOrderCommandHandler> logger)
    {
        _executionService = executionService;
        _inventoryStore = inventoryStore;
        _reportPublisher = reportPublisher;
        _hashService = hashService;
        _faultInjector = faultInjector;
        _nodeIdentity = nodeIdentity;
        _systemLog = systemLog;
        _logger = logger;
    }

    public async Task Handle(ExecuteProductionOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting production of {ComponentId} × {Amount} (order {CorrelationId})",
            request.ComponentId, request.Amount, request.CorrelationId);

        await _systemLog.PublishAsync("INF", "Production",
            $"Starting: {request.ComponentId} × {request.Amount} (order {request.CorrelationId})",
            cancellationToken);

        var manifest = ProductionManifest.Create(request.CorrelationId, request.ComponentId, request.Amount, request.Energy);
        var inventory = _inventoryStore.GetInventory();

        var faultMode = _faultInjector.CurrentMode.ToString();
        if (_faultInjector.CurrentMode != Cogfather.Node.Domain.Enums.FaultMode.None)
            _logger.LogWarning("Active fault mode: {FaultMode} for order {CorrelationId}", faultMode, request.CorrelationId);

        var result = await _executionService.ExecuteAsync(manifest, inventory);

        if (result.ErrorMessage == "Silent Failure occurred.")
        {
            _logger.LogWarning("Silent failure — suppressing report for order {CorrelationId}", request.CorrelationId);
            await _systemLog.PublishAsync("WRN", "Fault",
                $"Silent failure — no report for order {request.CorrelationId}", cancellationToken);
            return;
        }

        var hash = _hashService.GenerateHash(manifest);
        var finalHash = _faultInjector.TamperHash(hash);
        var hashTampered = finalHash != hash;

        await _reportPublisher.PublishProductionReportAsync(
            request.CorrelationId,
            _nodeIdentity.NodeId,
            result.ComponentId,
            result.AmountProduced,
            result.Success,
            finalHash,
            result.ErrorMessage);

        if (hashTampered)
        {
            _logger.LogWarning("Hash tampered for order {CorrelationId}", request.CorrelationId);
            await _systemLog.PublishAsync("WRN", "Fault",
                $"Hash tampered — report published with invalid hash for order {request.CorrelationId}", cancellationToken);
        }
        else if (!result.Success)
        {
            _logger.LogWarning("Production failed for {ComponentId} (order {CorrelationId}): {Error}",
                request.ComponentId, request.CorrelationId, result.ErrorMessage);
            await _systemLog.PublishAsync("WRN", "Production",
                $"Production failed: {request.ComponentId} — {result.ErrorMessage}", cancellationToken);
        }
        else
        {
            _logger.LogInformation("Production complete: {ComponentId} × {Amount} (order {CorrelationId}), report sent",
                result.ComponentId, result.AmountProduced, request.CorrelationId);
            await _systemLog.PublishAsync("INF", "Production",
                $"Complete: {result.ComponentId} × {result.AmountProduced} — report sent", cancellationToken);
        }
    }
}
