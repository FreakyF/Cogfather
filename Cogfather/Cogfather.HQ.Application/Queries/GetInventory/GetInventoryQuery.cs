using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetInventory;

public record GetInventoryQuery : IRequest<HqInventory>;