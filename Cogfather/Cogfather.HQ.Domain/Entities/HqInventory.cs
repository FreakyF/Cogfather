using Cogfather.HQ.Domain.Exceptions;

namespace Cogfather.HQ.Domain.Entities;

/// <summary>
/// The HQ-side inventory of produced components.
/// Items are added when production orders complete via consensus and consumed when
/// sub-order dependencies can be satisfied from existing stock.
/// </summary>
public class HqInventory
{
    /// <summary>EF Core surrogate key.</summary>
    public int Id { get; private set; }

    private readonly Dictionary<string, double> _items = new();

    /// <summary>Current stock levels keyed by component identifier.</summary>
    public IReadOnlyDictionary<string, double> Items => _items;

    /// <summary>
    /// Adds <paramref name="amount"/> units of <paramref name="componentId"/> to stock.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="amount"/> is negative.</exception>
    public void Add(string componentId, double amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (_items.ContainsKey(componentId))
            _items[componentId] += amount;
        else
            _items[componentId] = amount;
    }

    /// <summary>
    /// Removes <paramref name="amount"/> units of <paramref name="componentId"/> from stock.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="amount"/> is negative.</exception>
    /// <exception cref="InsufficientInventoryException">Thrown when stock is insufficient.</exception>
    public void Remove(string componentId, double amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (!_items.TryGetValue(componentId, out var currentAmount) || currentAmount < amount)
            throw new InsufficientInventoryException(componentId, amount);

        _items[componentId] -= amount;
    }
}