using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.Interfaces;
using Cogfather.HQ.Infrastructure.Adapters;
using Cogfather.HQ.Infrastructure.Data;
using Cogfather.HQ.Infrastructure.Identity;
using Cogfather.HQ.Infrastructure.Messaging;
using Cogfather.HQ.Infrastructure.Repositories;
using Cogfather.HQ.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Cogfather.HQ.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<HqDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContextFactory<HqDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Scoped);

        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("AuthConnection")
                              ?? configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
        services.AddScoped<IProductionReportRepository, ProductionReportRepository>();
        services.AddScoped<INodeRepository, NodeRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        services.AddSingleton<IRecipeBook, JsonRecipeBook>();
        services.AddSingleton<IProductionCatalog, ProductionCatalog>();

        services.AddTransient<DbSeeder>();
        services.AddSingleton<TotpService>();
        services.AddSingleton<CaptchaService>();

        services.AddSignalR();
        services.AddSingleton<IConsensusNotifier, SignalRConsensusNotifier>();

        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var options = configuration.GetSection("RabbitMQ").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
            return new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
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

        services.AddScoped<IOrderDispatcher, RabbitMqPublisher>();
        services.AddScoped<FaultControlPublisher>();

        services.AddHostedService<RabbitMqConsumerService>();
        services.AddHostedService<HeartbeatConsumerService>();

        services.AddHealthChecks()
            .AddDbContextCheck<HqDbContext>()
            .AddDbContextCheck<AuthDbContext>()
            .AddRabbitMQ(sp => sp.GetRequiredService<IConnection>());

        return services;
    }
}