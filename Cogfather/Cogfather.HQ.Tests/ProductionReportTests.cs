using System;
using Cogfather.HQ.Domain.Entities;
using Xunit;

namespace Cogfather.HQ.Tests;

public class ProductionReportTests
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange
        var nodeId = "node1";
        var recipeId = "recipe1";
        var success = true;

        // Act
        var report = new ProductionReport(nodeId, recipeId, success);

        // Assert
        Assert.NotEqual(Guid.Empty, report.Id);
        Assert.Equal(nodeId, report.NodeId);
        Assert.Equal(recipeId, report.RecipeId);
        Assert.Equal(success, report.Success);
        Assert.True(report.ReportedAt <= DateTime.UtcNow && report.ReportedAt > DateTime.UtcNow.AddSeconds(-5));
    }
}