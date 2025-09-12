using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Features.Databases.Shared;
using Pg.Explorer.Features.Queries.Shared;
using Pg.Explorer.Features.Tables.Shared;
using Pg.Explorer.Infrastructure.Database;
using Pg.Explorer.Infrastructure.Repositories;
using Pg.Explorer.Infrastructure.Seeding;
using Pg.Explorer.Infrastructure.Services.Connections;
using Pg.Explorer.Infrastructure.Services.Databases;
using Pg.Explorer.Infrastructure.Services.Queries;
using Pg.Explorer.Infrastructure.Services.Tables;
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
        services.AddScoped<IConnectionRepository, ConnectionRepository>();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IQueryService, QueryService>();
        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddScoped<ITableService, TableService>();

        return services;
    }
}