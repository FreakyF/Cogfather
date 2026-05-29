using Prometheus;

namespace Cogfather.HQ.Application.Metrics;

public static class CogfatherMetrics
{
    public static readonly Counter OrdersIssued = Prometheus.Metrics
        .CreateCounter("cogfather_orders_issued_total", "Total production orders issued", labelNames: ["recipe"]);

    public static readonly Counter ConsensusResults = Prometheus.Metrics
        .CreateCounter("cogfather_consensus_total", "Total consensus evaluations by verdict", labelNames: ["recipe", "verdict"]);

    public static readonly Counter ByzantineFaults = Prometheus.Metrics
        .CreateCounter("cogfather_byzantine_faults_total", "Total Byzantine faults detected per node", labelNames: ["node_id"]);

    public static readonly Gauge NodeReputation = Prometheus.Metrics
        .CreateGauge("cogfather_node_reputation", "Current node reputation score", labelNames: ["node_id", "display_name"]);

    public static readonly Counter NodeHeartbeats = Prometheus.Metrics
        .CreateCounter("cogfather_node_heartbeats_total", "Total heartbeats received per node", labelNames: ["node_id", "display_name"]);

    public static readonly Gauge InventoryQuantity = Prometheus.Metrics
        .CreateGauge("cogfather_inventory_quantity", "Current inventory quantity per item", labelNames: ["item"]);
}
