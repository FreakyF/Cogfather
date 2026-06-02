using System;
using Cogfather.Node.Application.Services;
using Cogfather.Node.Domain.ValueObjects;
using Xunit;

namespace Cogfather.Node.Tests;

public class ManifestHashServiceTests
{
    [Fact]
    public void GenerateHash_SameManifest_ReturnsSameHash()
    {
        // Arrange
        var service = new ManifestHashService();
        var correlationId = Guid.NewGuid();
        var manifest1 = ProductionManifest.Create(correlationId, "comp1", 10);
        var manifest2 = ProductionManifest.Create(correlationId, "comp1", 10);

        // Act
        var hash1 = service.GenerateHash(manifest1);
        var hash2 = service.GenerateHash(manifest2);

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.False(string.IsNullOrWhiteSpace(hash1));
    }

    [Fact]
    public void GenerateHash_DifferentManifest_ReturnsDifferentHash()
    {
        // Arrange
        var service = new ManifestHashService();
        var correlationId = Guid.NewGuid();
        var manifest1 = ProductionManifest.Create(correlationId, "comp1", 10);
        var manifest2 = ProductionManifest.Create(correlationId, "comp1", 11);

        // Act
        var hash1 = service.GenerateHash(manifest1);
        var hash2 = service.GenerateHash(manifest2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
}