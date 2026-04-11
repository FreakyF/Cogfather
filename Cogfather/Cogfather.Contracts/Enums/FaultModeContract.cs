namespace Cogfather.Contracts.Enums;

/// <summary>
/// Fault injection modes that HQ can instruct a node to activate.
/// Sent as part of FaultControlMessage.
/// Currently also configurable via environment variable NODE_FAULT_MODE.
/// </summary>
public enum FaultModeContract
{
    None = 0,
    DataManipulation = 1,
    SilentFailure = 2,
    HashTampering = 3,
    InventoryLie = 4,
    DelayedResponse = 5
}