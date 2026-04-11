using Cogfather.HQ.Domain.Enums;
using MediatR;

namespace Cogfather.HQ.Application.Commands.SetNodeFault;

public record SetNodeFaultCommand(string NodeId, FaultMode FaultMode) : IRequest;