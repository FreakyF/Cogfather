using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Queries.GetAllNodes;

public class GetAllNodesQueryHandler : IRequestHandler<GetAllNodesQuery, IEnumerable<NodeRegistration>>
{
    private readonly INodeRepository _nodeRepository;

    public GetAllNodesQueryHandler(INodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public Task<IEnumerable<NodeRegistration>> Handle(GetAllNodesQuery request, CancellationToken cancellationToken)
    {
        return _nodeRepository.GetAllAsync(cancellationToken);
    }
}