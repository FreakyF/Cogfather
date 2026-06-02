using Cogfather.Node.Application.Configuration;
using Cogfather.Node.Application.Interfaces;
using Cogfather.Node.Application.Services;
using Cogfather.Node.Domain.Interfaces;
using Cogfather.Node.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cogfather.Node.Application.Extensions;

public static class NodeApplicationServiceExtensions
{
    public static IServiceCollection AddNodeApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FaultConfiguration>(configuration.GetSection("FaultInjection"));

        services.AddSingleton<IFaultInjector, FaultInjector>();
        services.AddSingleton<IManifestHashService, ManifestHashService>();
        services.AddTransient<ProductionExecution>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NodeApplicationServiceExtensions).Assembly));

        return services;
    }
}