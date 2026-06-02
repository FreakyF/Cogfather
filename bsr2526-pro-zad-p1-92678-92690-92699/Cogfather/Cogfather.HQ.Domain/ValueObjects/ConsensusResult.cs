using Cogfather.HQ.Domain.Enums;

namespace Cogfather.HQ.Domain.ValueObjects;

/// <summary>
/// Immutable result produced by <see cref="Cogfather.HQ.Domain.Interfaces.IConsensusEngine"/> after
/// evaluating a full set of production reports for one order.
/// </summary>
/// <param name="RecipeId">Recipe the consensus was run for.</param>
/// <param name="Verdict">Approved, Rejected, or Inconclusive.</param>
/// <param name="Accuracy">Fraction of reports that agreed with the majority (0–1).</param>
/// <param name="ByzantineNodeIds">Node IDs that voted against the majority (empty when Inconclusive).</param>
public record ConsensusResult(
    string RecipeId,
    ConsensusVerdict Verdict,
    double Accuracy,
    IReadOnlyList<string> ByzantineNodeIds)
{
    public ConsensusResult(string recipeId, ConsensusVerdict verdict, double accuracy)
        : this(recipeId, verdict, accuracy, []) { }
}