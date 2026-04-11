using Cogfather.HQ.Domain.Entities;

namespace Cogfather.HQ.Application.Interfaces;

public interface IProductionOrderRepository
{
    Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ProductionOrder order, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductionOrder order, CancellationToken cancellationToken = default);
}