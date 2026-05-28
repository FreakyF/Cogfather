namespace Cogfather.Node.Application.Interfaces;

public interface ISystemLogPublisher
{
    Task PublishAsync(string level, string category, string message, CancellationToken cancellationToken = default);
}
