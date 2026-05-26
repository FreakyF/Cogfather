using Cogfather.HQ.Domain.Entities;

namespace Cogfather.HQ.Application.Interfaces;

public interface IProductionReportRepository
{
    Task AddAsync(ProductionReport report, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductionReport>> GetByCorrelationIdAsync(Guid correlationId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductionReport>> GetByRecipeIdAsync(string recipeId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductionReport>> GetAllAsync(CancellationToken cancellationToken = default);
}