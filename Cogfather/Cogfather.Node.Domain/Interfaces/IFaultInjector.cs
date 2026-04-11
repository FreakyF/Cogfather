using Cogfather.Node.Domain.Enums;
using Cogfather.Node.Domain.Records;
using Cogfather.Node.Domain.ValueObjects;

namespace Cogfather.Node.Domain.Interfaces;

public interface IFaultInjector
{
    FaultMode CurrentMode { get; }
    void SetFaultMode(FaultMode mode);

    Task ApplyDelayIfNeededAsync();
    bool ShouldSilentlyFail();
    ProductionManifest ManipulateManifest(ProductionManifest original);
    ProductionResult ManipulateResult(ProductionResult original);
    int GetLieInventoryCount(string componentId, int actualCount);
    string TamperHash(string actualHash);
}