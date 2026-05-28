namespace Cogfather.Node.Infrastructure.Messaging;

public class NodeRabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string OrdersExchange { get; set; } = "cogfather.orders";
    public string ReportsExchange { get; set; } = "cogfather.reports";
    public string FaultsExchange { get; set; } = "cogfather.faults";
    public string LogsExchange { get; set; } = "cogfather.logs";
    public string HeartbeatQueue { get; set; } = "cogfather.hq.heartbeats";
    public int HeartbeatIntervalSeconds { get; set; } = 10;
}