using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cogfather.HQ.Infrastructure.Repositories;

public class ProductionOrderRepository : IProductionOrderRepository
{
    private readonly HqDbContext _dbContext;

    public ProductionOrderRepository(HqDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductionOrders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductionOrders.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductionOrder order, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductionOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductionOrder order, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductionOrders.Update(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}