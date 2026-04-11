using Cogfather.Node.Domain.ValueObjects;

namespace Cogfather.Node.Application.Interfaces;

public interface IManifestHashService
{
    string GenerateHash(ProductionManifest manifest);
}