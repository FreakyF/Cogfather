using Cogfather.Node.Domain.ValueObjects;
using Cogfather.Node.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

var nodeId = builder.Configuration["NodeSettings:NodeId"] ?? "Unknown-Node";
var nodeIdentity = NodeIdentity.Create(nodeId);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Node", nodeId)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Node}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
builder.Services.AddSerilog();

builder.Services.AddSingleton(nodeIdentity);
builder.Services.AddNodeInfrastructureServices(builder.Configuration);

var host = builder.Build();

Log.Information("Worker Node {NodeId} starting up...", nodeId);

await host.RunAsync();