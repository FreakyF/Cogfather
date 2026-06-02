using Cogfather.HQ.Domain.Entities;

namespace Cogfather.HQ.Application.Interfaces;

/// <summary>
/// Persistence contract for production orders.
/// </summary>
public interface IProductionOrderRepository
{
    /// <summary>Returns the order with the given <paramref name="id"/>, or <see langword="null"/>.</summary>
    Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all production orders.</summary>
    Task<IEnumerable<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists a new production order.</summary>
    Task AddAsync(ProductionOrder order, CancellationToken cancellationToken = default);

    /// <summary>Persists status changes to an existing production order.</summary>
    Task UpdateAsync(ProductionOrder order, CancellationToken cancellationToken = default);
}
