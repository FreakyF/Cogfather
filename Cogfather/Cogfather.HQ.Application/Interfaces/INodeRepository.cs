using Cogfather.HQ.Domain.Entities;

namespace Cogfather.HQ.Application.Interfaces;

/// <summary>
/// Persistence contract for worker node registrations.
/// </summary>
public interface INodeRepository
{
    /// <summary>
    /// Returns the <see cref="NodeRegistration"/> with the given <paramref name="nodeId"/>,
    /// or <see langword="null"/> if the node is unknown.
    /// </summary>
    Task<NodeRegistration?> GetByIdAsync(string nodeId, CancellationToken cancellationToken = default);

    /// <summary>Returns all registered nodes.</summary>
    Task<IEnumerable<NodeRegistration>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists a newly registered node.</summary>
    Task AddAsync(NodeRegistration node, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing node (status, fault mode, reputation).</summary>
    Task UpdateAsync(NodeRegistration node, CancellationToken cancellationToken = default);
}
