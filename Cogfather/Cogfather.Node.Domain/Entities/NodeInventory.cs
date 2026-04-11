using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cogfather.Node.Domain.Entities;

public class NodeInventory
{
    private readonly ConcurrentDictionary<string, int> _items = new();

    public void AddComponent(string componentId, int amount)
    {
        if (string.IsNullOrWhiteSpace(componentId))
            throw new ArgumentException("ComponentId cannot be null or whitespace.", nameof(componentId));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
            
        _items.AddOrUpdate(componentId, amount, (_, existing) => existing + amount);
    }

    public bool TryTakeComponent(string componentId, int amount)
    {
        if (string.IsNullOrWhiteSpace(componentId))
            throw new ArgumentException("ComponentId cannot be null or whitespace.", nameof(componentId));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        
        while (true)
        {
            if (!_items.TryGetValue(componentId, out int current) || current < amount)
                return false;

            int newValue = current - amount;
            if (_items.TryUpdate(componentId, newValue, current))
                return true;
        }
    }

    public int GetCount(string componentId)
    {
        if (string.IsNullOrWhiteSpace(componentId))
            throw new ArgumentException("ComponentId cannot be null or whitespace.", nameof(componentId));
            
        return _items.TryGetValue(componentId, out int count) ? count : 0;
    }
    
    public IReadOnlyDictionary<string, int> GetAll()
    {
        return _items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
