using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetInventory;

public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, HqInventory>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetInventoryQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public Task<HqInventory> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
    {
        return _inventoryRepository.GetAsync(cancellationToken);
    }
}