using Cogfather.Node.Application.Interfaces;
using Cogfather.Node.Domain.Entities;

namespace Cogfather.Node.Infrastructure.Storage;

public class InMemoryInventoryStore : IInventoryStore
{
    private readonly NodeInventory _inventory = new();

    public NodeInventory GetInventory()
    {
        return _inventory;
    }
}