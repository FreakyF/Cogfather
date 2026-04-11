using System;

namespace Cogfather.Node.Domain.ValueObjects;

public record NodeIdentity(string NodeId)
{
    public static NodeIdentity Create(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            throw new ArgumentException("NodeId cannot be null or whitespace.", nameof(nodeId));
            
        return new NodeIdentity(nodeId);
    }
}
