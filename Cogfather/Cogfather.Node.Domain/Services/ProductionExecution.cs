using Cogfather.Node.Domain.Entities;
using Cogfather.Node.Domain.Interfaces;
using Cogfather.Node.Domain.Records;
using Cogfather.Node.Domain.ValueObjects;

namespace Cogfather.Node.Domain.Services;

public class ProductionExecution
{
    private readonly IFaultInjector _faultInjector;

    public ProductionExecution(IFaultInjector faultInjector)
    {
        _faultInjector = faultInjector;
    }

    public async Task<ProductionResult> ExecuteAsync(ProductionManifest manifest, NodeInventory inventory)
    {
        if (manifest.Energy > 0)
            await Task.Delay(TimeSpan.FromSeconds(manifest.Energy));

        await _faultInjector.ApplyDelayIfNeededAsync();

        if (_faultInjector.ShouldSilentlyFail())
            return ProductionResult.Failed(manifest.ComponentId, manifest.Amount, "Silent Failure occurred.");

        var actualManifest = _faultInjector.ManipulateManifest(manifest);

        inventory.AddComponent(actualManifest.ComponentId, actualManifest.Amount);

        var result = ProductionResult.Successful(actualManifest.ComponentId, actualManifest.Amount);

        return _faultInjector.ManipulateResult(result);
    }
}