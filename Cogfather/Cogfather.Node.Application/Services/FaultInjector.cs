using Cogfather.Node.Application.Configuration;
using Cogfather.Node.Domain.Enums;
using Cogfather.Node.Domain.Interfaces;
using Cogfather.Node.Domain.Records;
using Cogfather.Node.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Cogfather.Node.Application.Services;

public class FaultInjector : IFaultInjector
{
    private readonly FaultConfiguration _config;
    private readonly Random _random = new();

    public FaultInjector(IOptions<FaultConfiguration> config)
    {
        _config = config?.Value ?? new FaultConfiguration();
    }

    public FaultMode CurrentMode { get; private set; } = FaultMode.None;

    public void SetFaultMode(FaultMode mode)
    {
        CurrentMode = mode;
    }

    public async Task ApplyDelayIfNeededAsync()
    {
        if (CurrentMode == FaultMode.DelayedResponse) await Task.Delay(_config.DelayMilliseconds);
    }

    public bool ShouldSilentlyFail()
    {
        if (CurrentMode == FaultMode.SilentFailure) return _random.NextDouble() < _config.SilentFailureProbability;
        return false;
    }

    public ProductionManifest ManipulateManifest(ProductionManifest original)
    {
        if (CurrentMode == FaultMode.DataManipulation)
            return original with
            {
                Amount = original.Amount + _config.ManipulatedAmountOffset,
                ComponentId = original.ComponentId + _config.ManipulatedComponentIdSuffix
            };
        return original;
    }

    public ProductionResult ManipulateResult(ProductionResult original)
    {
        return original;
    }

    public int GetLieInventoryCount(string componentId, int actualCount)
    {
        if (CurrentMode == FaultMode.InventoryLie) return Math.Max(0, actualCount + _config.InventoryLieOffset);
        return actualCount;
    }

    public string TamperHash(string actualHash)
    {
        if (CurrentMode == FaultMode.HashTampering)
        {
            // Altering the hash for tampering fault
            if (actualHash.Length >= 8)
                return "DEADBEEF" + actualHash.Substring(8);

            return "DEADBEEF";
        }

        return actualHash;
    }
}