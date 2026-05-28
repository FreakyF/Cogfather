using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Cogfather.HQ.Domain.Interfaces;
using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Application.Services;

/// <summary>
///     Byzantine Fault Tolerant consensus engine.
///     Requires a strict majority of more than 2/3 of reports to agree for a verdict.
///     With n = 3f+1 nodes, tolerates up to f Byzantine faults.
/// </summary>
public class ConsensusEngine : IConsensusEngine
{
    private const double QuorumThreshold = 2.0 / 3.0;

    public Task<ConsensusResult> EvaluateAsync(string recipeId, IEnumerable<ProductionReport> reports)
    {
        var reportList = reports.ToList();

        if (reportList.Count == 0)
            return Task.FromResult(new ConsensusResult(recipeId, ConsensusVerdict.Inconclusive, 0));

        var total = reportList.Count;
        var successCount = reportList.Count(r => r.Success);
        var failureCount = total - successCount;

        var accuracy = (double)Math.Max(successCount, failureCount) / total;

        ConsensusVerdict verdict;
        IReadOnlyList<string> byzantineIds;
        if (successCount > total * QuorumThreshold)
        {
            verdict = ConsensusVerdict.Approved;
            byzantineIds = reportList.Where(r => !r.Success).Select(r => r.NodeId).ToList();
        }
        else if (failureCount > total * QuorumThreshold)
        {
            verdict = ConsensusVerdict.Rejected;
            byzantineIds = reportList.Where(r => r.Success).Select(r => r.NodeId).ToList();
        }
        else
        {
            verdict = ConsensusVerdict.Inconclusive;
            byzantineIds = [];
        }

        return Task.FromResult(new ConsensusResult(recipeId, verdict, accuracy, byzantineIds));
    }
}