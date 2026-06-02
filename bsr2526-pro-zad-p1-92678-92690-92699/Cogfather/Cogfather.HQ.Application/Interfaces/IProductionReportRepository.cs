using Cogfather.HQ.Domain.Entities;

namespace Cogfather.HQ.Application.Interfaces;

/// <summary>
/// Persistence contract for node production reports.
/// </summary>
public interface IProductionReportRepository
{
    /// <summary>Persists a production report submitted by a worker node.</summary>
    Task AddAsync(ProductionReport report, CancellationToken cancellationToken = default);

    /// <summary>Returns all reports belonging to a single production order.</summary>
    Task<IEnumerable<ProductionReport>> GetByCorrelationIdAsync(Guid correlationId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all reports for a given recipe across all orders.</summary>
    Task<IEnumerable<ProductionReport>> GetByRecipeIdAsync(string recipeId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns every report in the store.</summary>
    Task<IEnumerable<ProductionReport>> GetAllAsync(CancellationToken cancellationToken = default);
}
