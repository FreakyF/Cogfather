using Cogfather.HQ.Infrastructure.Services;
using Serilog.Core;
using Serilog.Events;

namespace Cogfather.HQ.Infrastructure.Serilog;

public sealed class SystemLogSink : ILogEventSink
{
    private readonly SystemLogService _service;

    public SystemLogSink(SystemLogService service)
    {
        _service = service;
    }

    public void Emit(LogEvent logEvent)
    {
        // Only include application-level logs, skip framework noise
        var sourceContext = logEvent.Properties.TryGetValue("SourceContext", out var sc)
            ? sc.ToString().Trim('"')
            : "";

        if (!sourceContext.StartsWith("Cogfather."))
            return;

        var level = logEvent.Level switch
        {
            LogEventLevel.Warning => "WRN",
            LogEventLevel.Error or LogEventLevel.Fatal => "ERR",
            _ => "INF"
        };

        var category = sourceContext.Split('.') is { Length: >= 3 } parts
            ? parts[2]  // e.g. "Application", "Infrastructure"
            : "HQ";

        _service.Add(new SystemLogEntry
        {
            Source = "HQ",
            Level = level,
            Category = category,
            Message = logEvent.RenderMessage(),
            Timestamp = logEvent.Timestamp.UtcDateTime
        });
    }
}
