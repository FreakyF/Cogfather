namespace Cogfather.Node.Application.Interfaces;

public interface IReportPublisher
{
    Task PublishProductionReportAsync(Guid correlationId, string nodeId, string componentId, int amountProduced,
        bool success, string hash, string errorMessage = null);
}