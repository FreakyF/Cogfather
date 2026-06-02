using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Domain.Interfaces;

/// <summary>
/// Evaluates a set of production reports and produces a Byzantine-fault-tolerant consensus verdict.
/// </summary>
public interface IConsensusEngine
{
    /// <summary>
    /// Analyses <paramref name="reports"/> using a 2/3 quorum threshold and returns the resulting
    /// <see cref="ConsensusResult"/>, including any node IDs identified as Byzantine outliers.
    /// </summary>
    /// <param name="recipeId">The recipe identifier the reports relate to.</param>
    /// <param name="reports">All reports collected for a single production order.</param>
    Task<ConsensusResult> EvaluateAsync(string recipeId, IEnumerable<ProductionReport> reports);
}
