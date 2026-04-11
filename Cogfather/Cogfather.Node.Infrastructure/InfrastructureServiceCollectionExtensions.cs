using System.Text.Json.Serialization.Metadata;
using Cogfather.Node.Infrastructure.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Dodaj to!

namespace Cogfather.Node.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddNodeInfrastructure(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ManufactureComponentConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                var nodeId = config["NodeSettings:NodeId"] ?? "GenericNode";

                cfg.ConfigureJsonSerializerOptions(options =>
                {
                    options.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                    return options;
                });

                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint($"manufacture-component-{nodeId}",
                    e => { e.ConfigureConsumer<ManufactureComponentConsumer>(context); });
            });
        });

        return services;
    }
}