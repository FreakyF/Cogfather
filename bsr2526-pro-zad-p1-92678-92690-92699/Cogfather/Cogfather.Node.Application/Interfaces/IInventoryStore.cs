using Cogfather.Node.Domain.Entities;

namespace Cogfather.Node.Application.Interfaces;

public interface IInventoryStore
{
    NodeInventory GetInventory();
}