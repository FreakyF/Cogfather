namespace Cogfather.HQ.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string OrdersExchange { get; set; } = "cogfather.orders";
    public string ReportsExchange { get; set; } = "cogfather.reports";
    public string FaultsExchange { get; set; } = "cogfather.faults";
    public string HeartbeatQueue { get; set; } = "cogfather.hq.heartbeats";
    public string ReportsQueue { get; set; } = "cogfather.hq.reports";
}