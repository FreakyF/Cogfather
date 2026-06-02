using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cogfather.HQ.Infrastructure.Repositories;

public class NodeRepository : INodeRepository
{
    private readonly HqDbContext _dbContext;

    public NodeRepository(HqDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NodeRegistration?> GetByIdAsync(string nodeId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Nodes.FirstOrDefaultAsync(n => n.NodeId == nodeId, cancellationToken);
    }

    public async Task<IEnumerable<NodeRegistration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Nodes.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(NodeRegistration node, CancellationToken cancellationToken = default)
    {
        _dbContext.Nodes.Add(node);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(NodeRegistration node, CancellationToken cancellationToken = default)
    {
        _dbContext.Nodes.Update(node);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}