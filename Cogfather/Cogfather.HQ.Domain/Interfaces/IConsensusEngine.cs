using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Domain.Interfaces;

public interface IConsensusEngine
{
    Task<ConsensusResult> EvaluateAsync(string recipeId, IEnumerable<ProductionReport> reports);
}