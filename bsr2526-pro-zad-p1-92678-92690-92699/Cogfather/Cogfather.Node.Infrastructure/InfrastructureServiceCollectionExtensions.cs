using Cogfather.Node.Application.Extensions;
using Cogfather.Node.Application.Interfaces;
using Cogfather.Node.Infrastructure.Messaging;
using Cogfather.Node.Infrastructure.Services;
using Cogfather.Node.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Cogfather.Node.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddNodeInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddNodeApplicationServices(configuration);

        services.Configure<NodeRabbitMqOptions>(configuration.GetSection("RabbitMq"));

        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var options = configuration.GetSection("RabbitMq").Get<NodeRabbitMqOptions>() ?? new NodeRabbitMqOptions();
            return new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password
            };
        });

        services.AddSingleton<IConnection>(sp =>
        {
            var factory = sp.GetRequiredService<IConnectionFactory>();
            var logger = sp.GetRequiredService<ILogger<IConnection>>();
            var policy = PollyPolicies.GetRetryPolicy(logger);

            return policy.ExecuteAsync(async () => await factory.CreateConnectionAsync()).GetAwaiter().GetResult();
        });

        services.AddSingleton<IInventoryStore, InMemoryInventoryStore>();
        services.AddScoped<IReportPublisher, RabbitMqReportPublisher>();
        services.AddScoped<ISystemLogPublisher, RabbitMqSystemLogPublisher>();

        services.AddHostedService<RabbitMqOrderConsumerService>();
        services.AddHostedService<RabbitMqHeartbeatService>();
        services.AddHostedService<RabbitMqFaultControlConsumerService>();

        return services;
    }
}