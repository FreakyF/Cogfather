using System.Collections.Concurrent;

namespace Cogfather.HQ.Infrastructure.Services;

public sealed class SystemLogEntry
{
    public string Source { get; init; } = "HQ";
    public string Level { get; init; } = "INF";
    public string Category { get; init; } = "General";
    public string Message { get; init; } = "";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public sealed class SystemLogService
{
    private const int MaxEntries = 300;
    private readonly ConcurrentQueue<SystemLogEntry> _buffer = new();

    public event Action<SystemLogEntry>? OnLogEntry;

    public IReadOnlyList<SystemLogEntry> GetRecent(int count = 100)
    {
        var all = _buffer.ToArray();
        return all.Length <= count ? all : all[^count..];
    }

    public void Add(SystemLogEntry entry)
    {
        _buffer.Enqueue(entry);
        while (_buffer.Count > MaxEntries)
            _buffer.TryDequeue(out _);
        OnLogEntry?.Invoke(entry);
    }
}
