using Microsoft.EntityFrameworkCore;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Domain.Queries.Entities;

namespace Pg.Explorer.Infrastructure.Database;

public class PgExplorerDbContext(DbContextOptions<PgExplorerDbContext> options) : DbContext(options)
{
    public DbSet<ConnectionConfig> ConnectionConfigs { get; set; }
    public DbSet<QueryEntity> Queries { get; set; }
}