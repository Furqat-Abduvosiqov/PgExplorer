using System.Data;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.ConnectionConfigs;
using Pg.Explorer.Features.ConnectionConfigs.Shared.Services;
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
        if (connection is null)
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

    public async Task<ErrorOr<IEnumerable<SchemaInfo>>> GetSchemasAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound(code: "ConnectionConfig.NotFound",
                description: $"Connection config {connectionId} not found.");

        var schemas = new List<SchemaInfo>();

        try
        {
            var connectionString = connectionService.GetConnectionString(connection);

            await using var npgsqlConnection = new NpgsqlConnection(connectionString);
            await npgsqlConnection.OpenAsync(cancellationToken);

            var query = @"
                    SELECT 
                        schema_name as name,
                        schema_owner as owner,
                        (SELECT COUNT(*) 
                         FROM information_schema.tables 
                         WHERE table_schema = schema_name) as table_count
                    FROM information_schema.schemata
                    WHERE schema_name NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                    ORDER BY schema_name";

            await using var command = new NpgsqlCommand(query, npgsqlConnection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                schemas.Add(new SchemaInfo
                (
                    reader.GetString("name"),
                    reader.GetString("owner"),
                    reader.GetInt32("table_count")
                ));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving schemas for database {DatabaseName}", databaseName);
            return Error.Failure(
                code: "Error retrieving data",
                description: $"Could not get schema: {ex.Message}");
        }

        return schemas;
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

    public async Task<ErrorOr<bool>> CreateDatabaseAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound(code: "ConnectionConfig.NotFound",
                description: $"Connection config {connectionId} not found.");

        try
        {
            await using var npgsqlConnection = new NpgsqlConnection(connectionService.GetConnectionString(connection));
            await npgsqlConnection.OpenAsync(cancellationToken);

            var query = $"CREATE DATABASE \"{databaseName}\"";

            await using var command = new NpgsqlCommand(query, npgsqlConnection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation("Created database: {DatabaseName}", databaseName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating database {DatabaseName}", databaseName);
            return false;
        }
    }

    public Task<ErrorOr<bool>> DropDatabaseAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}