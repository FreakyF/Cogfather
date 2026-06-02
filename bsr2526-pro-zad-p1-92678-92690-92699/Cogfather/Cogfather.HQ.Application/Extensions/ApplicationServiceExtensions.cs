using System.Reflection;
using Cogfather.HQ.Application.Behaviors;
using Cogfather.HQ.Application.Services;
using Cogfather.HQ.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Cogfather.HQ.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddSingleton<IConsensusEngine, ConsensusEngine>();
        services.AddSingleton<QuorumReportCollector>();

        return services;
    }
}