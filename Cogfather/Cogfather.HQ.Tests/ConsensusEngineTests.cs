using System.Collections.Generic;
using System.Threading.Tasks;
using Cogfather.HQ.Application.Services;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Xunit;

namespace Cogfather.HQ.Tests;

public class ConsensusEngineTests
{
    [Fact]
    public async Task EvaluateAsync_NoReports_ReturnsInconclusive()
    {
        // Arrange
        var engine = new ConsensusEngine();
        var reports = new List<ProductionReport>();

        // Act
        var result = await engine.EvaluateAsync("recipe1", reports);

        // Assert
        Assert.Equal(ConsensusVerdict.Inconclusive, result.Verdict);
        Assert.Equal(0, result.Accuracy);
    }

    [Theory]
    [InlineData(3, 3, ConsensusVerdict.Approved, 1.0)]
    [InlineData(3, 0, ConsensusVerdict.Rejected, 1.0)]
    [InlineData(3, 2, ConsensusVerdict.Inconclusive, 2.0 / 3.0)] // 2/3 is not > 2/3
    [InlineData(4, 3, ConsensusVerdict.Approved, 0.75)] // 3/4 = 0.75 > 2/3 (0.66)
    [InlineData(4, 1, ConsensusVerdict.Rejected, 0.75)] // 3 failures
    [InlineData(1, 1, ConsensusVerdict.Approved, 1.0)]
    [InlineData(10, 7, ConsensusVerdict.Approved, 0.7)] // 7/10 = 0.7 > 0.66
    [InlineData(10, 6, ConsensusVerdict.Inconclusive, 0.6)] // 6/10 = 0.6 < 0.66
    public async Task EvaluateAsync_WithReports_ReturnsExpectedVerdict(int total, int successCount,
        ConsensusVerdict expectedVerdict, double expectedAccuracy)
    {
        // Arrange
        var engine = new ConsensusEngine();
        var reports = new List<ProductionReport>();
        for (var i = 0; i < successCount; i++)
            reports.Add(new ProductionReport(Guid.NewGuid(), $"node{i}", "recipe1", true));
        for (var i = successCount; i < total; i++)
            reports.Add(new ProductionReport(Guid.NewGuid(), $"node{i}", "recipe1", false));

        // Act
        var result = await engine.EvaluateAsync("recipe1", reports);

        // Assert
        Assert.Equal(expectedVerdict, result.Verdict);
        Assert.Equal(expectedAccuracy, result.Accuracy, 5);
    }
}