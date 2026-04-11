namespace Cogfather.Node.Domain.Enums;

public enum FaultMode
{
    None = 0,
    DataManipulation = 1,
    SilentFailure = 2,
    HashTampering = 3,
    InventoryLie = 4,
    DelayedResponse = 5
}