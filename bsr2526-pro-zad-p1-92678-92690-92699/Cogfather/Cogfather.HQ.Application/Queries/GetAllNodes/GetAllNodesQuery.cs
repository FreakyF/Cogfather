using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetAllNodes;

public record GetAllNodesQuery : IRequest<IEnumerable<NodeRegistration>>;