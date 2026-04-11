using Cogfather.HQ.Domain.Exceptions;

namespace Cogfather.HQ.Domain.Entities;

public class HqInventory
{
    private readonly Dictionary<string, double> _items = new();

    public IReadOnlyDictionary<string, double> Items => _items;

    public void Add(string componentId, double amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (_items.ContainsKey(componentId))
            _items[componentId] += amount;
        else
            _items[componentId] = amount;
    }

    public void Remove(string componentId, double amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (!_items.TryGetValue(componentId, out var currentAmount) || currentAmount < amount)
            throw new InsufficientInventoryException(componentId, amount);

        _items[componentId] -= amount;
    }
}