using Cogfather.HQ.Domain.Entities;

namespace Cogfather.HQ.Application.Interfaces;

/// <summary>
/// Persistence contract for the HQ inventory store.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>Returns the current inventory snapshot.</summary>
    Task<HqInventory> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists a full inventory snapshot (overwrites existing state).</summary>
    Task SaveAsync(HqInventory inventory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically adds <paramref name="amount"/> units of <paramref name="componentId"/> to the
    /// stored inventory, safe for concurrent callers.
    /// </summary>
    Task AddItemAsync(string componentId, double amount, CancellationToken cancellationToken = default);
}
