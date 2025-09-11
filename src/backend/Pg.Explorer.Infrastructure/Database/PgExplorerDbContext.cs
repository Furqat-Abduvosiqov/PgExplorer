using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.NameTranslation;
using Pg.Explorer.Domain.Connections;
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

    public DbSet<Connection> Connections { get; set; }
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

        modelBuilder.Entity<QueryEntity>()
            .HasOne(q => q.Connection)
            .WithMany(c => c.Queries)
            .HasForeignKey(q => q.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void MapEnum<TEnum>()
        where TEnum : struct, Enum
    {
        string enumName = NameTranslator.TranslateTypeName(typeof(TEnum).Name);
        NpgsqlConnection.GlobalTypeMapper.MapEnum<TEnum>(enumName, NameTranslator);
    }
}