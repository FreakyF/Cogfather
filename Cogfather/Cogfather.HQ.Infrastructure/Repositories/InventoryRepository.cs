using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cogfather.HQ.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly HqDbContext _dbContext;

    public InventoryRepository(HqDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HqInventory> GetAsync(CancellationToken cancellationToken = default)
    {
        var inventory = await _dbContext.Inventories.FirstOrDefaultAsync(cancellationToken);
        if (inventory == null)
        {
            inventory = new HqInventory();
            _dbContext.Inventories.Add(inventory);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return inventory;
    }

    public async Task SaveAsync(HqInventory inventory, CancellationToken cancellationToken = default)
    {
        _dbContext.Inventories.Update(inventory);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}