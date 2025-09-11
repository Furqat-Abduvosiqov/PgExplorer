using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pg.Explorer.Domain.ConnectionConfigs;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Infrastructure.Database;
using Pg.Explorer.Infrastructure.Repositories;
using Pg.Explorer.Infrastructure.Seeding;
using Pg.Explorer.Shared.Encryptions;

namespace Pg.Explorer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var postgresConnectionString = configuration.GetConnectionString("Postgres");

        services.AddDbContext<PgExplorerDbContext>(x => x
            .EnableSensitiveDataLogging()
            .UseNpgsql(postgresConnectionString)
            .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<SeedService>();
        services.AddScoped<IQueryEntityRepository, QueryEntityRepository>();
        services.AddScoped<IConnectionConfigRepository, ConnectionConfigRepository>();

        return services;
    }
}