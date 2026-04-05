using Cogfather.Node.Infrastructure;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

var nodeId = builder.Configuration["NodeSettings:NodeId"] ?? "Unknown-Node";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Node", nodeId)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Node}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
builder.Services.AddSerilog();

builder.Services.AddNodeInfrastructure();

var host = builder.Build();

Log.Information("Worker Node starting up...");

await host.RunAsync();