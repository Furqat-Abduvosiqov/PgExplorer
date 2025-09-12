using System.Data;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Features.Databases.Shared;
using Pg.Explorer.Features.Tables.Shared;

namespace Pg.Explorer.Infrastructure.Services.Databases;

public class DatabaseService(
    IConnectionRepository connectionRepository,
    ILogger<DatabaseService> logger,
    IConnectionService connectionService)
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

    public async Task<ErrorOr<IEnumerable<TableInfo>>> GetTablesAsync(long connectionId, string schemaName,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(connectionId, cancellationToken);

        if (connection is null)
            return Error.NotFound(code: "ConnectionConfig.NotFound",
                description: $"Connection config {connectionId} not found.");

        var tables = new List<TableInfo>();

        try
        {
            var connectionString = connectionService.GetConnectionString(connection);

            await using var npgsqlConnection = new NpgsqlConnection(connectionString);
            await npgsqlConnection.OpenAsync(cancellationToken);

            var query = @"
            SELECT 
                pt.schemaname AS schema_name,
                pt.tablename AS table_name,
                'TABLE' AS table_type,
                COALESCE(pstat.n_tup_ins, 0) - COALESCE(pstat.n_tup_del, 0) AS row_count,
                pg_size_pretty(
                    pg_total_relation_size(
                        (quote_ident(pt.schemaname) || '.' || quote_ident(pt.tablename))::regclass
                    )
                ) AS size
            FROM pg_tables pt
            LEFT JOIN pg_stat_user_tables pstat 
                ON pt.schemaname = pstat.schemaname 
               AND pt.tablename = pstat.relname
            WHERE pt.schemaname = @schemaName

            UNION ALL

            SELECT 
                v.schemaname AS schema_name,
                v.viewname AS table_name,
                'VIEW' AS table_type,
                0 AS row_count,
                '0 bytes' AS size
            FROM pg_views v
            WHERE v.schemaname = @schemaName

            ORDER BY table_name";

            await using var command = new NpgsqlCommand(query, npgsqlConnection);
            command.Parameters.AddWithValue("@schemaName", schemaName);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                tables.Add(
                    new TableInfo
                    {
                        SchemaName = reader.GetString("schema_name"),
                        TableName = reader.GetString("table_name"),
                        TableType = reader.GetString("table_type"),
                        RowCount = reader.GetInt64("row_count"),
                        Size = reader.GetString("size")
                    });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tables for schema {SchemaName}", schemaName);

            return Error.Failure(
                code: "Error retrieving data",
                description: $"Could not get tables: {ex.Message}");
        }

        return tables;
    }

    public async Task<ErrorOr<TableInfo>> GetTableInfoAsync(long connectionId, string schemaName, string tableName,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound(code: "ConnectionConfig.NotFound",
                description: $"Connection config {connectionId} not found.");

        TableInfo? tableInfo = null;

        try
        {
            await using var npgsqlConnection = new NpgsqlConnection(connectionService.GetConnectionString(connection));
            await npgsqlConnection.OpenAsync(cancellationToken);

            var tableQuery = @"
                    SELECT 
                        t.schemaname as schema_name,
                        t.tablename as table_name,
                        'TABLE' as table_type,
                        COALESCE(s.n_tup_ins, 0) - COALESCE(s.n_tup_del, 0) as row_count,
                        pg_size_pretty(pg_total_relation_size(quote_ident(t.schemaname)||'.'||quote_ident(t.tablename))) as size
                    FROM pg_tables t
                    LEFT JOIN pg_stat_user_tables s ON t.schemaname = s.schemaname AND t.tablename = s.relname
                    WHERE t.schemaname = @schemaName AND t.tablename = @tableName";

            await using var tableCommand = new NpgsqlCommand(tableQuery, npgsqlConnection);
            tableCommand.Parameters.AddWithValue("@schemaName", schemaName);
            tableCommand.Parameters.AddWithValue("@tableName", tableName);

            await using var tableReader = await tableCommand.ExecuteReaderAsync(cancellationToken);
            if (await tableReader.ReadAsync(cancellationToken))
            {
                tableInfo = new TableInfo
                {
                    SchemaName = tableReader.GetString("schema_name"),
                    TableName = tableReader.GetString("table_name"),
                    TableType = tableReader.GetString("table_type"),
                    RowCount = tableReader.GetInt64("row_count"),
                    Size = tableReader.GetString("size")
                };
            }

            await tableReader.CloseAsync();

            if (tableInfo is not null)
            {
                var columnsQuery = @"
                        SELECT 
                            c.column_name,
                            c.data_type,
                            c.is_nullable::boolean as is_nullable,
                            c.column_default,
                            c.ordinal_position,
                            c.character_maximum_length,
                            CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END as is_primary_key,
                            CASE WHEN fk.column_name IS NOT NULL THEN true ELSE false END as is_foreign_key
                        FROM information_schema.columns c
                        LEFT JOIN (
                            SELECT kcu.column_name
                            FROM information_schema.table_constraints tc
                            JOIN information_schema.key_column_usage kcu 
                                ON tc.constraint_name = kcu.constraint_name
                                AND tc.table_schema = kcu.table_schema
                            WHERE tc.constraint_type = 'PRIMARY KEY'
                                AND tc.table_schema = @schemaName
                                AND tc.table_name = @tableName
                        ) pk ON c.column_name = pk.column_name
                        LEFT JOIN (
                            SELECT kcu.column_name
                            FROM information_schema.table_constraints tc
                            JOIN information_schema.key_column_usage kcu 
                                ON tc.constraint_name = kcu.constraint_name
                                AND tc.table_schema = kcu.table_schema
                            WHERE tc.constraint_type = 'FOREIGN KEY'
                                AND tc.table_schema = @schemaName
                                AND tc.table_name = @tableName
                        ) fk ON c.column_name = fk.column_name
                        WHERE c.table_schema = @schemaName AND c.table_name = @tableName
                        ORDER BY c.ordinal_position";

                await using var columnsCommand = new NpgsqlCommand(columnsQuery, npgsqlConnection);
                columnsCommand.Parameters.AddWithValue("@schemaName", schemaName);
                columnsCommand.Parameters.AddWithValue("@tableName", tableName);

                await using var columnsReader = await columnsCommand.ExecuteReaderAsync(cancellationToken);
                while (await columnsReader.ReadAsync(cancellationToken))
                {
                    tableInfo.Columns.Add(new ColumnInfo
                    {
                        ColumnName = columnsReader.GetString("column_name"),
                        DataType = columnsReader.GetString("data_type"),
                        IsNullable = columnsReader.GetBoolean("is_nullable"),
                        DefaultValue = await columnsReader.IsDBNullAsync("column_default", cancellationToken)
                            ? null
                            : columnsReader.GetString("column_default"),
                        OrdinalPosition = columnsReader.GetInt32("ordinal_position"),
                        MaxLength = await columnsReader.IsDBNullAsync("character_maximum_length", cancellationToken)
                            ? null
                            : columnsReader.GetInt32("character_maximum_length"),
                        IsPrimaryKey = columnsReader.GetBoolean("is_primary_key"),
                        IsForeignKey = columnsReader.GetBoolean("is_foreign_key")
                    });
                }

                await columnsReader.CloseAsync();


                var indexesQuery = @"
                        SELECT 
                            i.relname as index_name,
                            am.amname as index_type,
                            ix.indisunique as is_unique,
                            ix.indisprimary as is_primary,
                            array_agg(a.attname ORDER BY array_position(ix.indkey, a.attnum)) as columns
                        FROM pg_class t
                        JOIN pg_index ix ON t.oid = ix.indrelid
                        JOIN pg_class i ON i.oid = ix.indexrelid
                        JOIN pg_am am ON i.relam = am.oid
                        JOIN pg_namespace n ON t.relnamespace = n.oid
                        JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = ANY(ix.indkey)
                        WHERE n.nspname = @schemaName AND t.relname = @tableName
                        GROUP BY i.relname, am.amname, ix.indisunique, ix.indisprimary
                        ORDER BY i.relname";

                await using var indexesCommand = new NpgsqlCommand(indexesQuery, npgsqlConnection);
                indexesCommand.Parameters.AddWithValue("@schemaName", schemaName);
                indexesCommand.Parameters.AddWithValue("@tableName", tableName);

                await using var indexesReader = await indexesCommand.ExecuteReaderAsync(cancellationToken);
                while (await indexesReader.ReadAsync(cancellationToken))
                {
                    var columns = (string[])indexesReader.GetValue("columns");
                    tableInfo.Indexes.Add(new IndexInfo
                    {
                        IndexName = indexesReader.GetString("index_name"),
                        IndexType = indexesReader.GetString("index_type"),
                        IsUnique = indexesReader.GetBoolean("is_unique"),
                        IsPrimary = indexesReader.GetBoolean("is_primary"),
                        Columns = columns.ToList()
                    });
                }
            }
            else
            {
                return Error.NotFound(
                    code: "Tables.NotFound",
                    description: $"Table with name: {tableName} not found.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving table info for {SchemaName}.{TableName}", schemaName, tableName);
            return Error.Unexpected(description: $"Exception occured: {ex.Message}");
        }

        return tableInfo;
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

    public async Task<ErrorOr<bool>> DropDatabaseAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        if (connection == null)
            return Error.NotFound(
                code: "ConnectionConfig.NotFound",
                description: $"Connection config {connectionId} not found.");

        try
        {
            await using var npgsqlConnection = new NpgsqlConnection(connectionService.GetConnectionString(connection));
            await npgsqlConnection.OpenAsync(cancellationToken);

            var terminateQuery = @"
                    SELECT pg_terminate_backend(pg_stat_activity.pid)
                    FROM pg_stat_activity
                    WHERE pg_stat_activity.datname = @databaseName
                      AND pid <> pg_backend_pid()";

            await using var terminateCommand = new NpgsqlCommand(terminateQuery, npgsqlConnection);
            terminateCommand.Parameters.AddWithValue("@databaseName", databaseName);
            await terminateCommand.ExecuteNonQueryAsync(cancellationToken);


            var dropQuery = $"DROP DATABASE \"{databaseName}\"";
            await using var dropCommand = new NpgsqlCommand(dropQuery, npgsqlConnection);
            await dropCommand.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation("Dropped database: {DatabaseName}", databaseName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error dropping database {DatabaseName}", databaseName);
            return false;
        }
    }
}