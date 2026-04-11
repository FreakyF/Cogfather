using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Application.Interfaces;

public interface IOrderDispatcher
{
    Task DispatchAsync(ProductionOrder order, Recipe recipe, CancellationToken cancellationToken = default);
}