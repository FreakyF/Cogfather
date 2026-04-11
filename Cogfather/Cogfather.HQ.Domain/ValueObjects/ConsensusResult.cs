using Cogfather.HQ.Domain.Enums;

namespace Cogfather.HQ.Domain.ValueObjects;

public record ConsensusResult(string RecipeId, ConsensusVerdict Verdict, double Accuracy);