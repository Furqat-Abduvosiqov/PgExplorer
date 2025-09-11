using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pg.Explorer.Features.Connections.CreateConnection;
using Pg.Explorer.Features.Connections.Shared.Services;
using Pg.Explorer.Features.Databases.Shared.Services;
using Pg.Explorer.Features.Queries.Shared.Services;
using Pg.Explorer.Features.Tables.Shared.Services;

namespace Pg.Explorer.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.RegisterEndpointsFromAssemblyContaining<CreateConnectionEndpoint>();

        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IQueryService, QueryService>();
        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddScoped<ITableService, TableService>();

        return services;
    }

    public static WebApplication MapFeatures(this WebApplication app)
    {
        app.MapEndpoints();
        return app;
    }
}