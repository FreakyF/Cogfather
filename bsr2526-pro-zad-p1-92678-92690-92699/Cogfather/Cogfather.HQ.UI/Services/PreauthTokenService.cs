using System.Collections.Concurrent;

namespace Cogfather.HQ.UI.Services;

public sealed class PreauthTokenService
{
    private readonly ConcurrentDictionary<string, (string UserId, DateTime Expires)> _tokens = new();

    public string CreateToken(string userId)
    {
        var token = Guid.NewGuid().ToString("N");
        _tokens[token] = (userId, DateTime.UtcNow.AddSeconds(30));
        return token;
    }

    public string? ConsumeToken(string token)
    {
        if (_tokens.TryRemove(token, out var entry) && entry.Expires > DateTime.UtcNow)
            return entry.UserId;
        return null;
    }
}
