using Cogfather.Node.Domain.Enums;
using Cogfather.Node.Domain.Records;
using Cogfather.Node.Domain.ValueObjects;

namespace Cogfather.Node.Domain.Interfaces;

/// <summary>
/// Controls Byzantine fault injection on a worker node.
/// All fault modes are set by HQ via RabbitMQ and applied transparently during production.
/// </summary>
public interface IFaultInjector
{
    /// <summary>The currently active fault mode.</summary>
    FaultMode CurrentMode { get; }

    /// <summary>Switches the active fault mode.</summary>
    void SetFaultMode(FaultMode mode);

    /// <summary>Suspends execution for the configured delay when <see cref="FaultMode.DelayedResponse"/> is active.</summary>
    Task ApplyDelayIfNeededAsync();

    /// <summary>Returns <see langword="true"/> when the node should silently drop its report.</summary>
    bool ShouldSilentlyFail();

    /// <summary>Optionally corrupts the manifest data values when <see cref="FaultMode.DataManipulation"/> is active.</summary>
    ProductionManifest ManipulateManifest(ProductionManifest original);

    /// <summary>Optionally corrupts the production result when <see cref="FaultMode.DataManipulation"/> is active.</summary>
    ProductionResult ManipulateResult(ProductionResult original);

    /// <summary>Returns a manipulated inventory count used to lie to HQ about available stock.</summary>
    int GetLieInventoryCount(string componentId, int actualCount);

    /// <summary>Returns a tampered hash string when <see cref="FaultMode.HashTampering"/> is active.</summary>
    string TamperHash(string actualHash);
}