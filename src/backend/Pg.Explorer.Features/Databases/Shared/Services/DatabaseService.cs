using System.Data;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Features.ConnectionConfigs.Shared.Services;
using Pg.Explorer.Features.Schemas.Shared;
using Pg.Explorer.Features.Tables.Shared;

namespace Pg.Explorer.Features.Databases.Shared.Services;

public class DatabaseService(
    IConnectionConfigRepository connectionRepository,
    ILogger<DatabaseService> logger,
    IConnectionConfigService connectionService)
    : IDatabaseService
{
    public async Task<ErrorOr<IEnumerable<DatabaseInfo>>> GetDatabasesAsync(long connectionId,
        CancellationToken ctx = default)
    {
        var connection = await connectionRepository.GetByIdAsync(connectionId, ctx);
        if (connection == null)
            return Error.NotFound(code: "ConnectionConfig.NotFound",
                description: $"Connection config {connectionId} not found.");

        var databases = new List<DatabaseInfo>();
        var connectionString = connectionService.GetConnectionString(connection);

        try
        {
            await using var npgsqlConnection = new NpgsqlConnection(connectionString);
            await npgsqlConnection.OpenAsync(ctx);

            var query = @"
            SELECT 
                d.datname AS name,
                pg_catalog.pg_get_userbyid(d.datdba) AS owner,
                pg_catalog.pg_encoding_to_char(d.encoding) AS encoding,
                CASE 
                    WHEN pg_catalog.has_database_privilege(d.datname, 'CONNECT')
                    THEN pg_catalog.pg_database_size(d.datname)
                    ELSE NULL
                END AS size,
                COALESCE(tc.tables_count, 0) AS tables_count
            FROM pg_catalog.pg_database d
            LEFT JOIN (
                SELECT table_catalog, COUNT(*) AS tables_count
                FROM information_schema.tables
                WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
                GROUP BY table_catalog
            ) tc ON tc.table_catalog = d.datname
            WHERE d.datistemplate = false
            ORDER BY d.datname;";

            await using var command = new NpgsqlCommand(query, npgsqlConnection);
            await using var reader = await command.ExecuteReaderAsync(ctx);

            while (await reader.ReadAsync(ctx))
            {
                databases.Add(new DatabaseInfo
                (
                    reader.GetString("name"),
                    reader.GetString("owner"),
                    await reader.IsDBNullAsync("size", ctx) ? 0 : reader.GetInt64("size"),
                    reader.GetString("encoding"),
                    reader.GetInt32(reader.GetOrdinal("tables_count"))
                ));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving databases for connection {ConnectionId}", connectionId);

            return Error.Failure(
                code: "Error retrieving data",
                description: $"Could not get databases: {ex.Message}");
        }

        return databases;
    }

    public Task<ErrorOr<IEnumerable<SchemaInfo>>> GetSchemasAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorOr<IEnumerable<TableInfo>>> GetTablesAsync(long connectionId, string schemaName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorOr<TableInfo?>> GetTableInfoAsync(long connectionId, string schemaName, string tableName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorOr<bool>> CreateDatabaseAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorOr<bool>> DropDatabaseAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}