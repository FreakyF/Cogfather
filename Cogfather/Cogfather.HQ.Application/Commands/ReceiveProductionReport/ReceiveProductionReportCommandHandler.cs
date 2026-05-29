using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Application.Metrics;
using Cogfather.HQ.Application.Services;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Cogfather.HQ.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cogfather.HQ.Application.Commands.ReceiveProductionReport;

public class ReceiveProductionReportCommandHandler : IRequestHandler<ReceiveProductionReportCommand>
{
    private readonly IProductionCatalog _catalog;
    private readonly IConsensusEngine _consensusEngine;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<ReceiveProductionReportCommandHandler> _logger;
    private readonly INodeRepository _nodeRepository;
    private readonly IConsensusNotifier _notifier;
    private readonly IProductionOrderRepository _orderRepository;
    private readonly QuorumReportCollector _quorumCollector;
    private readonly IProductionReportRepository _reportRepository;

    public ReceiveProductionReportCommandHandler(
        IProductionReportRepository reportRepository,
        IProductionOrderRepository orderRepository,
        IProductionCatalog catalog,
        IInventoryRepository inventoryRepository,
        IConsensusEngine consensusEngine,
        IConsensusNotifier notifier,
        QuorumReportCollector quorumCollector,
        INodeRepository nodeRepository,
        ILogger<ReceiveProductionReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _orderRepository = orderRepository;
        _catalog = catalog;
        _inventoryRepository = inventoryRepository;
        _consensusEngine = consensusEngine;
        _notifier = notifier;
        _quorumCollector = quorumCollector;
        _nodeRepository = nodeRepository;
        _logger = logger;
    }

    public async Task Handle(ReceiveProductionReportCommand request, CancellationToken cancellationToken)
    {
        var report = new ProductionReport(request.OrderId, request.NodeId, request.RecipeId, request.Success);
        await _reportRepository.AddAsync(report, cancellationToken);

        _logger.LogInformation("Report received: order={OrderId} node={NodeId} success={Success}",
            request.OrderId, request.NodeId[..Math.Min(8, request.NodeId.Length)], request.Success);

        var reports = (await _reportRepository.GetByCorrelationIdAsync(request.OrderId, cancellationToken)).ToList();

        if (!await _quorumCollector.HasQuorumAsync(reports, cancellationToken))
            return;

        _logger.LogInformation("Quorum reached for order {OrderId} ({Count} reports)", request.OrderId, reports.Count);

        var result = await _consensusEngine.EvaluateAsync(request.RecipeId, reports);

        _logger.LogInformation("Consensus for {RecipeId}: {Verdict} (accuracy={Accuracy:P0})",
            request.RecipeId, result.Verdict, result.Accuracy);
        CogfatherMetrics.ConsensusResults.WithLabels(request.RecipeId, result.Verdict.ToString()).Inc();

        foreach (var byzantineNodeId in result.ByzantineNodeIds)
        {
            var node = await _nodeRepository.GetByIdAsync(byzantineNodeId, cancellationToken);
            if (node is not null)
            {
                node.RecordByzantineFault();
                await _nodeRepository.UpdateAsync(node, cancellationToken);
                var shortId = byzantineNodeId[..Math.Min(8, byzantineNodeId.Length)];
                CogfatherMetrics.ByzantineFaults.WithLabels(shortId).Inc();
                CogfatherMetrics.NodeReputation.WithLabels(shortId, node.Address).Set(node.ReputationScore);
                _logger.LogWarning("Byzantine fault recorded for node {NodeId}: reputation={Score}, faults={Count}",
                    shortId, node.ReputationScore, node.ByzantineFaultCount);
            }
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is not null && order.Status == ProductionOrderStatus.InProgress)
        {
            if (result.Verdict == ConsensusVerdict.Approved)
            {
                order.CompleteProduction();
                await AddToInventoryAsync(order.RecipeId, order.TargetAmount, cancellationToken);
                _logger.LogInformation("Order {OrderId} completed: {RecipeId} × {Amount}", order.Id, order.RecipeId, order.TargetAmount);
            }
            else
            {
                order.FailProduction();
                _logger.LogWarning("Order {OrderId} failed: Byzantine fault detected for {RecipeId}", order.Id, order.RecipeId);
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        await _notifier.NotifyAsync(result, cancellationToken);
    }

    private async Task AddToInventoryAsync(string recipeId, double targetAmount, CancellationToken cancellationToken)
    {
        var recipe = await _catalog.GetRecipeAsync(recipeId, cancellationToken);
        if (recipe == null) return;

        var primaryProduct = recipe.Products.FirstOrDefault(p => p.ComponentId == recipeId)
                             ?? recipe.Products.FirstOrDefault();
        if (primaryProduct == null) return;

        var outputPerCraft = primaryProduct.Amount;
        var craftsNeeded = Math.Ceiling(targetAmount / outputPerCraft);
        var actualProduced = craftsNeeded * outputPerCraft;

        await _inventoryRepository.AddItemAsync(primaryProduct.ComponentId, actualProduced, cancellationToken);
    }
}