using System.Text.Json.Serialization;
using Cogfather.Contracts.Events;
using Cogfather.Contracts.Messages.Faults;
using Cogfather.Contracts.Messages.Heartbeat;
using Cogfather.Contracts.Messages.Orders;
using Cogfather.Contracts.Messages.Reports;

namespace Cogfather.Contracts;

[JsonSerializable(typeof(NodeHeartbeatMessage))]
[JsonSerializable(typeof(ProductionOrderMessage))]
[JsonSerializable(typeof(FaultControlMessage))]
[JsonSerializable(typeof(ProductionReportMessage))]
[JsonSerializable(typeof(SystemLogEvent))]
[JsonSerializable(typeof(Dictionary<string, int>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class CogfatherJsonContext : JsonSerializerContext;