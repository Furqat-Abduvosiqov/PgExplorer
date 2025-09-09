using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Infrastructure.Database;
using Pg.Explorer.Infrastructure.Repositories;

namespace Pg.Explorer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var postgresConnectionString = configuration.GetConnectionString("Postgres");

        services.AddDbContext<PgExplorerDbContext>(x => x
            .EnableSensitiveDataLogging()
            .UseNpgsql(postgresConnectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__MyMigrationsHistory", "pg_explorer"))
            .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IQueryEntityRepository, QueryEntityRepository>();
        services.AddScoped<IConnectionConfigRepository, ConnectionConfigRepository>();

        return services;
    }
}