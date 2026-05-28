namespace Cogfather.HQ.Infrastructure.Services;

public sealed class InventoryUpdateLock
{
    public SemaphoreSlim Semaphore { get; } = new(1, 1);
}
