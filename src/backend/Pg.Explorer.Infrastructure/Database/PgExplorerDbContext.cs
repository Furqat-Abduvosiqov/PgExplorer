using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.NameTranslation;
using Pg.Explorer.Domain.ConnectionConfigs;
using Pg.Explorer.Domain.Queries.Entities;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Pg.Explorer.Infrastructure.Database;

public class PgExplorerDbContext(DbContextOptions<PgExplorerDbContext> options) : DbContext(options)
{
    private static readonly INpgsqlNameTranslator NameTranslator = new NpgsqlNullNameTranslator();

    static PgExplorerDbContext()
    {
        MapEnum<QueryType>();
        MapEnum<QueryStatus>();
    }

    public DbSet<ConnectionConfig> ConnectionConfigs { get; set; }
    public DbSet<QueryEntity> Queries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<QueryStatus>(nameTranslator: NameTranslator);
        modelBuilder.HasPostgresEnum<QueryType>(nameTranslator: NameTranslator);

        modelBuilder
            .Entity<QueryEntity>()
            .Property(q => q.QueryStatus)
            .HasConversion<string>()
            .HasColumnType("text");

        modelBuilder
            .Entity<QueryEntity>()
            .Property(q => q.QueryType)
            .HasConversion<string>()
            .HasColumnType("text");
    }

    private static void MapEnum<TEnum>()
        where TEnum : struct, Enum
    {
        string enumName = NameTranslator.TranslateTypeName(typeof(TEnum).Name);
        NpgsqlConnection.GlobalTypeMapper.MapEnum<TEnum>(enumName, NameTranslator);
    }
}