using Cogfather.HQ.Domain.Entities;

namespace Cogfather.HQ.Application.Interfaces;

public interface INodeRepository
{
    Task<NodeRegistration?> GetByIdAsync(string nodeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NodeRegistration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(NodeRegistration node, CancellationToken cancellationToken = default);
    Task UpdateAsync(NodeRegistration node, CancellationToken cancellationToken = default);
}