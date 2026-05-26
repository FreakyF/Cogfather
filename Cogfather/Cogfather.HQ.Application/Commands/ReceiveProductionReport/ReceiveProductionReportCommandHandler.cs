using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Application.Services;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Cogfather.HQ.Domain.Interfaces;
using MediatR;

namespace Cogfather.HQ.Application.Commands.ReceiveProductionReport;

public class ReceiveProductionReportCommandHandler : IRequestHandler<ReceiveProductionReportCommand>
{
    private readonly IConsensusEngine _consensusEngine;
    private readonly IConsensusNotifier _notifier;
    private readonly IProductionOrderRepository _orderRepository;
    private readonly QuorumReportCollector _quorumCollector;
    private readonly IProductionReportRepository _reportRepository;

    public ReceiveProductionReportCommandHandler(
        IProductionReportRepository reportRepository,
        IProductionOrderRepository orderRepository,
        IConsensusEngine consensusEngine,
        IConsensusNotifier notifier,
        QuorumReportCollector quorumCollector)
    {
        _reportRepository = reportRepository;
        _orderRepository = orderRepository;
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
                order.CompleteProduction();
            else
                order.FailProduction();

            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        await _notifier.NotifyAsync(result, cancellationToken);
    }
}