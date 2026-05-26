using Cogfather.Node.Domain.ValueObjects;
using Cogfather.Node.Infrastructure;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Services.AddSerilog(lc => lc.ReadFrom.Configuration(builder.Configuration));

var nodeId = builder.Configuration["NodeSettings:NodeId"]
    ?? throw new InvalidOperationException("NodeSettings:NodeId is not configured");

builder.Services.AddSingleton(NodeIdentity.Create(nodeId));
builder.Services.AddNodeInfrastructureServices(builder.Configuration);

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Worker Node {NodeId} starting up", nodeId);

await host.RunAsync();
