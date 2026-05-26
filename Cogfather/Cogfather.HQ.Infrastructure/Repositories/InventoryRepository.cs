using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cogfather.HQ.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbContextFactory<HqDbContext> _factory;

    public InventoryRepository(IDbContextFactory<HqDbContext> factory)
    {
        _factory = factory;
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
}
