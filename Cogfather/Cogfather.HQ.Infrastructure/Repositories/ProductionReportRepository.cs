using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cogfather.HQ.Infrastructure.Repositories;

public class ProductionReportRepository : IProductionReportRepository
{
    private readonly HqDbContext _dbContext;

    public ProductionReportRepository(HqDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ProductionReport report, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductionReports.Add(report);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductionReport>> GetByRecipeIdAsync(string recipeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductionReports
            .Where(r => r.RecipeId == recipeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductionReport>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductionReports.ToListAsync(cancellationToken);
    }
}