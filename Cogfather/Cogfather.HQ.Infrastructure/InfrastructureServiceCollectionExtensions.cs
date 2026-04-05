using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Infrastructure.Adapters;

namespace Cogfather.HQ.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddHqInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProductionCatalog, ProductionCatalog>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<Consumers.ComponentManufacturedConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}