using MediatR;

namespace Cogfather.HQ.Application.Commands.RegisterNode;

public record RegisterNodeCommand(string NodeId, string Address) : IRequest;