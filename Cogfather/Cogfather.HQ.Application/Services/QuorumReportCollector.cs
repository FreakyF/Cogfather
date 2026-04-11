using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;

namespace Cogfather.HQ.Application.Services;

/// <summary>
/// Determines whether a sufficient quorum of production reports has been collected
/// to proceed with BFT consensus evaluation.
/// In a system with n = 3f+1 nodes, a quorum requires at least 2f+1 reports.
/// </summary>
public class QuorumReportCollector
{
    private readonly INodeRepository _nodeRepository;

    public QuorumReportCollector(INodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task<bool> HasQuorumAsync(
        IEnumerable<ProductionReport> reports,
        CancellationToken cancellationToken = default)
    {
        var nodes = await _nodeRepository.GetAllAsync(cancellationToken);
        var activeCount = nodes.Count(n => n.Status == NodeStatus.Active);

        if (activeCount == 0)
            return true;

        var reportCount = reports.Count();

        // Minimum quorum: ceil((2n + 1) / 3) — the smallest set that rules out a Byzantine majority
        var requiredReports = (int)Math.Ceiling((2.0 * activeCount + 1.0) / 3.0);
        return reportCount >= requiredReports;
    }
}