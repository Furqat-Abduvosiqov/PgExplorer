using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pg.Explorer.Features.ConnectionConfigs.CreateConnectionConfig;
using Pg.Explorer.Features.ConnectionConfigs.Shared.Services;
using Pg.Explorer.Features.Queries.Shared.Services;

namespace Pg.Explorer.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.RegisterEndpointsFromAssemblyContaining<CreateConnectionConfigEndpoint>();

        services.AddScoped<IConnectionConfigService, ConnectionConfigService>();
        services.AddScoped<IQueryService, QueryService>();

        return services;
    }

    public static WebApplication MapFeatures(this WebApplication app)
    {
        app.MapEndpoints();
        return app;
    }
}