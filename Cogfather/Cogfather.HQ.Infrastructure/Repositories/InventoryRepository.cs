using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Infrastructure.Data;
using Cogfather.HQ.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Cogfather.HQ.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbContextFactory<HqDbContext> _factory;
    private readonly InventoryUpdateLock _lock;

    public InventoryRepository(IDbContextFactory<HqDbContext> factory, InventoryUpdateLock @lock)
    {
        _factory = factory;
        _lock = @lock;
    }

    public async Task<HqInventory> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var inventory = await db.Inventories.FirstOrDefaultAsync(cancellationToken);
        if (inventory != null) return inventory;

        inventory = new HqInventory();
        db.Inventories.Add(inventory);
        await db.SaveChangesAsync(cancellationToken);
        return inventory;
    }

    public async Task SaveAsync(HqInventory inventory, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        db.Inventories.Update(inventory);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddItemAsync(string componentId, double amount, CancellationToken cancellationToken = default)
    {
        await _lock.Semaphore.WaitAsync(cancellationToken);
        try
        {
            await using var db = await _factory.CreateDbContextAsync(cancellationToken);
            var inventory = await db.Inventories.FirstOrDefaultAsync(cancellationToken);
            if (inventory == null)
            {
                inventory = new HqInventory();
                db.Inventories.Add(inventory);
            }
            inventory.Add(componentId, amount);
            db.Inventories.Update(inventory);
            await db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _lock.Semaphore.Release();
        }
    }
}
