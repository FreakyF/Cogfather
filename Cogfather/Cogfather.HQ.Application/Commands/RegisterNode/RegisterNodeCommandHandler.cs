using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using MediatR;

namespace Cogfather.HQ.Application.Commands.RegisterNode;

public class RegisterNodeCommandHandler : IRequestHandler<RegisterNodeCommand>
{
    private readonly INodeRepository _nodeRepository;

    public RegisterNodeCommandHandler(INodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task Handle(RegisterNodeCommand request, CancellationToken cancellationToken)
    {
        var existing = await _nodeRepository.GetByIdAsync(request.NodeId, cancellationToken);
        if (existing is not null)
            return;

        var node = new NodeRegistration(request.NodeId, request.Address);
        await _nodeRepository.AddAsync(node, cancellationToken);
    }
}