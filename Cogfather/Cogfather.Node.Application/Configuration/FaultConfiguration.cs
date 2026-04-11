namespace Cogfather.Node.Application.Configuration;

public class FaultConfiguration
{
    public int DelayMilliseconds { get; set; } = 5000;
    public double SilentFailureProbability { get; set; } = 1.0;
    public int InventoryLieOffset { get; set; } = -10;
    public int ManipulatedAmountOffset { get; set; } = 5;
    public string ManipulatedComponentIdSuffix { get; set; } = "_TAMPERED";
}