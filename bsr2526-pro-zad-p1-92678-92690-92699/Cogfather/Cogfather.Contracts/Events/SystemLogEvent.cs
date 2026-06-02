namespace Cogfather.Contracts.Events;

public record SystemLogEvent(
    string NodeId,
    string Level,
    string Message,
    DateTime Timestamp
);