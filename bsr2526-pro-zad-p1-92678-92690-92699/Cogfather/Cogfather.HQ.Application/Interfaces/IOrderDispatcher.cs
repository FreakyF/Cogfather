using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Application.Interfaces;

/// <summary>
/// Dispatches a production order to all registered worker nodes via the message broker.
/// </summary>
public interface IOrderDispatcher
{
    /// <summary>
    /// Publishes <paramref name="order"/> to the fanout exchange so every active node receives it.
    /// </summary>
    Task DispatchAsync(ProductionOrder order, Recipe recipe, CancellationToken cancellationToken = default);
}
