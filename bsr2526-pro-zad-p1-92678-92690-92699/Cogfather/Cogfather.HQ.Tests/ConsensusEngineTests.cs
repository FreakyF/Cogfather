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
    [InlineData(3, 2, ConsensusVerdict.Approved, 2.0 / 3.0)]  // 2/3 >= 2/3 threshold → Approved
    [InlineData(4, 3, ConsensusVerdict.Approved, 0.75)]       // 3/4 >= 2/3 → Approved
    [InlineData(4, 2, ConsensusVerdict.Inconclusive, 0.5)]    // 2/4 < 2/3 in either direction → Inconclusive
    [InlineData(4, 1, ConsensusVerdict.Rejected, 0.75)]       // 3 failures >= 2/3
    [InlineData(1, 1, ConsensusVerdict.Approved, 1.0)]
    [InlineData(10, 7, ConsensusVerdict.Approved, 0.7)]       // 7/10 = 0.7 >= 2/3
    [InlineData(10, 6, ConsensusVerdict.Inconclusive, 0.6)]   // 6/10 = 0.6 < 2/3 in either direction
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

    [Fact]
    public async Task EvaluateAsync_Approved_ByzantineIdsAreFailingNodes()
    {
        var engine = new ConsensusEngine();
        // 3/4 = 0.75 > 2/3 threshold → Approved; node3 is the Byzantine minority
        var reports = new List<ProductionReport>
        {
            new(Guid.NewGuid(), "node0", "r", true),
            new(Guid.NewGuid(), "node1", "r", true),
            new(Guid.NewGuid(), "node2", "r", true),
            new(Guid.NewGuid(), "node3", "r", false),
        };

        var result = await engine.EvaluateAsync("r", reports);

        Assert.Equal(ConsensusVerdict.Approved, result.Verdict);
        Assert.Single(result.ByzantineNodeIds);
        Assert.Contains("node3", result.ByzantineNodeIds);
    }

    [Fact]
    public async Task EvaluateAsync_Rejected_ByzantineIdsAreSucceedingNodes()
    {
        var engine = new ConsensusEngine();
        // 3/4 failures = 0.75 > 2/3 threshold → Rejected; node3 is the Byzantine minority
        var reports = new List<ProductionReport>
        {
            new(Guid.NewGuid(), "node0", "r", false),
            new(Guid.NewGuid(), "node1", "r", false),
            new(Guid.NewGuid(), "node2", "r", false),
            new(Guid.NewGuid(), "node3", "r", true),
        };

        var result = await engine.EvaluateAsync("r", reports);

        Assert.Equal(ConsensusVerdict.Rejected, result.Verdict);
        Assert.Single(result.ByzantineNodeIds);
        Assert.Contains("node3", result.ByzantineNodeIds);
    }

    [Fact]
    public async Task EvaluateAsync_Inconclusive_NoByzantineIds()
    {
        var engine = new ConsensusEngine();
        // 2 success, 2 failure from 4 nodes: neither side reaches 2/3 threshold → Inconclusive
        var reports = new List<ProductionReport>
        {
            new(Guid.NewGuid(), "node0", "r", true),
            new(Guid.NewGuid(), "node1", "r", true),
            new(Guid.NewGuid(), "node2", "r", false),
            new(Guid.NewGuid(), "node3", "r", false),
        };

        var result = await engine.EvaluateAsync("r", reports);

        Assert.Equal(ConsensusVerdict.Inconclusive, result.Verdict);
        Assert.Empty(result.ByzantineNodeIds);
    }
}