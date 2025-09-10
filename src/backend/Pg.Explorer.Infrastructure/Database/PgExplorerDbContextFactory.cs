using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pg.Explorer.Infrastructure.Database;

public class PgExplorerDbContextFactory : IDesignTimeDbContextFactory<PgExplorerDbContext>
{
    public PgExplorerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PgExplorerDbContext>();

        // Default connection string for migrations
        // This can be overridden by setting the connection string in appsettings.json
        var connectionString = "Host=localhost;Database=PgExplorer;Username=postgres;Password=password";

        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new PgExplorerDbContext(optionsBuilder.Options);
    }
}