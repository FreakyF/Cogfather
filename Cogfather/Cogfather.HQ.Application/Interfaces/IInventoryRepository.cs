using Cogfather.HQ.Domain.Entities;

namespace Cogfather.HQ.Application.Interfaces;

public interface IInventoryRepository
{
    Task<HqInventory> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(HqInventory inventory, CancellationToken cancellationToken = default);
}