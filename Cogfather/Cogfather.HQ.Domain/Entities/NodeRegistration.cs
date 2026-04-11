using Cogfather.HQ.Domain.Enums;

namespace Cogfather.HQ.Domain.Entities;

public class NodeRegistration
{
    public NodeRegistration(string nodeId, string address)
    {
        NodeId = nodeId;
        Address = address;
        Status = NodeStatus.Active;
        FaultMode = FaultMode.None;
    }

    public string NodeId { get; private set; }
    public string Address { get; private set; }
    public NodeStatus Status { get; private set; }
    public FaultMode FaultMode { get; private set; }

    public void UpdateStatus(NodeStatus status)
    {
        Status = status;
    }

    public void SetFaultMode(FaultMode mode)
    {
        FaultMode = mode;
    }
}