using MediatR;

namespace Cogfather.HQ.Application.Commands.ClearNodeFault;

public record ClearNodeFaultCommand(string NodeId) : IRequest;