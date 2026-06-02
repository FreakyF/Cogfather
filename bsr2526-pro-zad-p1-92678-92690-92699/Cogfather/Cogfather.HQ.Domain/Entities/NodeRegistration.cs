using Cogfather.HQ.Domain.Enums;

namespace Cogfather.HQ.Domain.Entities;

/// <summary>
/// Represents a worker node that has registered with HQ.
/// Tracks liveness, active fault mode, and trust score derived from Byzantine fault history.
/// </summary>
public class NodeRegistration
{
    /// <summary>Creates a new registration in <see cref="NodeStatus.Active"/> state with full reputation.</summary>
    public NodeRegistration(string nodeId, string address)
    {
        NodeId = nodeId;
        Address = address;
        Status = NodeStatus.Active;
        FaultMode = FaultMode.None;
        ReputationScore = 100;
        ByzantineFaultCount = 0;
    }

    /// <summary>Unique node identifier (MD5-derived UUID from the node's configured ID string).</summary>
    public string NodeId { get; private set; }

    /// <summary>Network address used for administrative purposes.</summary>
    public string Address { get; private set; }

    /// <summary>Current liveness status of the node.</summary>
    public NodeStatus Status { get; private set; }

    /// <summary>Currently active fault injection mode (set by HQ for demo/testing).</summary>
    public FaultMode FaultMode { get; private set; }

    /// <summary>
    /// Trust score in the range [0, 100]. Starts at 100 and decreases by 20 for each confirmed
    /// Byzantine fault. Nodes with a low score are considered untrustworthy.
    /// </summary>
    public int ReputationScore { get; private set; }

    /// <summary>Total number of Byzantine faults attributed to this node by consensus.</summary>
    public int ByzantineFaultCount { get; private set; }

    /// <summary>Updates the node's liveness status.</summary>
    public void UpdateStatus(NodeStatus status) => Status = status;

    /// <summary>Sets the active fault injection mode.</summary>
    public void SetFaultMode(FaultMode mode) => FaultMode = mode;

    /// <summary>
    /// Records one Byzantine fault: increments <see cref="ByzantineFaultCount"/> and reduces
    /// <see cref="ReputationScore"/> by 20 (floor 0).
    /// </summary>
    public void RecordByzantineFault()
    {
        ByzantineFaultCount++;
        ReputationScore = Math.Max(0, ReputationScore - 20);
    }
}
