using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Application.Services;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Cogfather.HQ.Domain.Interfaces;
using MediatR;

namespace Cogfather.HQ.Application.Commands.ReceiveProductionReport;

public class ReceiveProductionReportCommandHandler : IRequestHandler<ReceiveProductionReportCommand>
{
    private readonly IProductionCatalog _catalog;
    private readonly IConsensusEngine _consensusEngine;
    private readonly IInventoryRepository _inventoryRepository;
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
        QuorumReportCollector quorumCollector)
    {
        _reportRepository = reportRepository;
        _orderRepository = orderRepository;
        _catalog = catalog;
        _inventoryRepository = inventoryRepository;
        _consensusEngine = consensusEngine;
        _notifier = notifier;
        _quorumCollector = quorumCollector;
    }

    public async Task Handle(ReceiveProductionReportCommand request, CancellationToken cancellationToken)
    {
        var report = new ProductionReport(request.OrderId, request.NodeId, request.RecipeId, request.Success);
        await _reportRepository.AddAsync(report, cancellationToken);

        var reports = (await _reportRepository.GetByCorrelationIdAsync(request.OrderId, cancellationToken)).ToList();

        if (!await _quorumCollector.HasQuorumAsync(reports, cancellationToken))
            return;

        var result = await _consensusEngine.EvaluateAsync(request.RecipeId, reports);

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is not null && order.Status == ProductionOrderStatus.InProgress)
        {
            if (result.Verdict == ConsensusVerdict.Approved)
            {
                order.CompleteProduction();
                await AddToInventoryAsync(order.RecipeId, order.TargetAmount, cancellationToken);
            }
            else
            {
                order.FailProduction();
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

        var inventory = await _inventoryRepository.GetAsync(cancellationToken);
        inventory.Add(primaryProduct.ComponentId, actualProduced);
        await _inventoryRepository.SaveAsync(inventory, cancellationToken);
    }
}