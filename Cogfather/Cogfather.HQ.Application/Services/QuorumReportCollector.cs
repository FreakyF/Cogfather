using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Cogfather.HQ.Application.Services;

/// <summary>
///     Determines whether a sufficient quorum of production reports has been collected
///     to proceed with BFT consensus evaluation.
///     In a system with n = 3f+1 nodes, a quorum requires at least 2f+1 reports.
/// </summary>
public class QuorumReportCollector
{
    private readonly IServiceScopeFactory _scopeFactory;

    public QuorumReportCollector(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<bool> HasQuorumAsync(
        IEnumerable<ProductionReport> reports,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var nodeRepository = scope.ServiceProvider.GetRequiredService<INodeRepository>();

        var nodes = await nodeRepository.GetAllAsync(cancellationToken);
        var activeCount = nodes.Count(n => n.Status == NodeStatus.Active);

        if (activeCount == 0)
            return true;

        var reportCount = reports.Count();

        // Minimum quorum: ceil((2n + 1) / 3) — the smallest set that rules out a Byzantine majority
        var requiredReports = (int)Math.Ceiling((2.0 * activeCount + 1.0) / 3.0);
        return reportCount >= requiredReports;
    }
}
