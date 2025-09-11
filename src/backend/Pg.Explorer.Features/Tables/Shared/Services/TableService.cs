using System.Diagnostics;
using System.Text;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Features.Connections.Shared.Services;
using Pg.Explorer.Features.Tables.ExportTable;
using Pg.Explorer.Features.Tables.GetTableData;
using Pg.Explorer.Features.Tables.InsertRow;

namespace Pg.Explorer.Features.Tables.Shared.Services;

public class TableService(
    IQueryEntityRepository queryRepository,
    IConnectionRepository connectionRepository,
    ILogger<TableService> logger,
    IConnectionService connectionService
) : ITableService
{
    public async Task<ErrorOr<ExecutionResult>> GetTableDataAsync(
        GetTableDataCommand request,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound("ConnectionConfig.NotFound",
                $"Connection config {request.ConnectionId} not found.");

        var query = QueryEntity.Create(connection.Id,
            $"SELECT * FROM \"{request.SchemaName}\".\"{request.TableName}\" LIMIT @limit OFFSET @offset",
            QueryType.Read);

        var executionResult = new ExecutionResult
        {
            QueryText = query.QueryBody,
            ExecutedAt = DateTimeOffset.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var npgsqlConnection =
                new NpgsqlConnection(connectionService.GetConnectionString(connection));
            await npgsqlConnection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(query.QueryBody, npgsqlConnection);
            command.Parameters.AddWithValue("@limit", request.PageSize);
            command.Parameters.AddWithValue("@offset", (request.Page - 1) * request.PageSize);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            // Columns
            for (int i = 0; i < reader.FieldCount; i++)
                executionResult.Columns.Add(reader.GetName(i));

            // Rows
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[executionResult.Columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);

                executionResult.Data.Add(row);
            }

            stopwatch.Stop();

            executionResult.RowsAffected = executionResult.Data.Count;
            executionResult.Success = true;
            executionResult.Message = $"Retrieved {executionResult.RowsAffected} rows (page {request.Page}).";
            executionResult.ExecutionTime = stopwatch.Elapsed;

            query.ExecutionTime = stopwatch.Elapsed;
            query.QueryStatus = QueryStatus.Success;
            query.ErrorMessage = null;
            query.ExecutedAt = DateTimeOffset.UtcNow;
            query.AffectedRows = executionResult.Data.Count;

            await queryRepository.AddAsync(query, cancellationToken);

            logger.LogInformation(
                "Retrieved {Count} rows from {Schema}.{Table}, page {Page}",
                executionResult.RowsAffected, request.SchemaName, request.TableName, request.Page);

            return executionResult;
        }
        catch (Exception e)
        {
            stopwatch.Stop();

            query.ExecutionTime = stopwatch.Elapsed;
            query.QueryStatus = QueryStatus.Error;
            query.ErrorMessage = e.Message;
            query.ExecutedAt = DateTimeOffset.UtcNow;

            await queryRepository.AddAsync(query, cancellationToken);

            logger.LogError(e, "Error retrieving data from {Schema}.{Table}",
                request.SchemaName, request.TableName);

            return Error.Failure("Tables.QueryError", $"Failed to retrieve table data: {e.Message}");
        }
        finally
        {
            await queryRepository.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ErrorOr<ExecutionResult>> InsertRowAsync(
        InsertRowCommand request,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound("ConnectionConfig.NotFound",
                $"Connection config {request.ConnectionId} not found.");

        var columns = string.Join(", ", request.RowData.Keys.Select(k => $"\"{k}\""));
        var parameters = string.Join(", ", request.RowData.Keys.Select((_, i) => $"@p{i}"));
        var query = $"INSERT INTO \"{request.SchemaName}\".\"{request.TableName}\" ({columns}) VALUES ({parameters})";

        try
        {
            await using var npgsqlConnection =
                new NpgsqlConnection(connectionService.GetConnectionString(connection));
            await npgsqlConnection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(query, npgsqlConnection);

            int i = 0;
            foreach (var kvp in request.RowData)
                command.Parameters.AddWithValue($"@p{i++}", kvp.Value ?? DBNull.Value);

            var affected = await command.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation("Inserted row into {Schema}.{Table}, {Count} row(s) affected",
                request.SchemaName, request.TableName, affected);

            return new ExecutionResult
            {
                QueryText = query,
                ExecutedAt = DateTimeOffset.UtcNow,
                Success = true,
                RowsAffected = affected,
                Message = $"Inserted {affected} row(s)."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inserting row into {Schema}.{Table}", request.SchemaName, request.TableName);
            return Error.Failure("Tables.InsertError", $"Insert failed: {ex.Message}");
        }
    }

    public async Task<ErrorOr<ExecutionResult>> UpdateRowAsync(
        UpdateRowRequest request,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound("ConnectionConfig.NotFound",
                $"Connection config {request.ConnectionId} not found.");

        var setClause = string.Join(", ", request.RowData.Keys.Select((k, i) => $"\"{k}\" = @s{i}"));
        var whereClause = string.Join(" AND ", request.WhereConditions.Keys.Select((k, i) => $"\"{k}\" = @w{i}"));
        var query = $"UPDATE \"{request.SchemaName}\".\"{request.TableName}\" SET {setClause} WHERE {whereClause}";

        try
        {
            await using var npgsqlConnection =
                new NpgsqlConnection(connectionService.GetConnectionString(connection));
            await npgsqlConnection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(query, npgsqlConnection);

            int si = 0;
            foreach (var kvp in request.RowData)
                command.Parameters.AddWithValue($"@s{si++}", kvp.Value ?? DBNull.Value);

            int wi = 0;
            foreach (var kvp in request.WhereConditions)
                command.Parameters.AddWithValue($"@w{wi++}", kvp.Value ?? DBNull.Value);

            var affected = await command.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation("Updated {Count} row(s) in {Schema}.{Table}",
                affected, request.SchemaName, request.TableName);

            return new ExecutionResult
            {
                QueryText = query,
                ExecutedAt = DateTimeOffset.UtcNow,
                Success = true,
                RowsAffected = affected,
                Message = $"Updated {affected} row(s)."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating row in {Schema}.{Table}", request.SchemaName, request.TableName);
            return Error.Failure("Tables.UpdateError", $"Update failed: {ex.Message}");
        }
    }

    public async Task<ErrorOr<ExecutionResult>> DeleteRowAsync(
        DeleteRowRequest request,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound("ConnectionConfig.NotFound",
                $"Connection config {request.ConnectionId} not found.");

        var whereClause = string.Join(" AND ", request.WhereConditions.Keys.Select((k, i) => $"\"{k}\" = @p{i}"));
        var query = $"DELETE FROM \"{request.SchemaName}\".\"{request.TableName}\" WHERE {whereClause}";

        try
        {
            await using var npgsqlConnection = new NpgsqlConnection(connectionService.GetConnectionString(connection));
            await npgsqlConnection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(query, npgsqlConnection);

            int i = 0;
            foreach (var kvp in request.WhereConditions)
                command.Parameters.AddWithValue($"@p{i++}", kvp.Value ?? DBNull.Value);

            var affected = await command.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation("Deleted {Count} row(s) from {Schema}.{Table}",
                affected, request.SchemaName, request.TableName);

            return new ExecutionResult
            {
                QueryText = query,
                ExecutedAt = DateTimeOffset.UtcNow,
                Success = true,
                RowsAffected = affected,
                Message = $"Deleted {affected} row(s)."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting row(s) from {Schema}.{Table}", request.SchemaName, request.TableName);
            return Error.Failure("Tables.DeleteError", $"Delete failed: {ex.Message}");
        }
    }

    public async Task<ErrorOr<string>> ExportTableToCsvAsync(
        ExportTableRequest request,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound("ConnectionConfig.NotFound",
                $"Connection config {request.ConnectionId} not found.");

        var csv = new StringBuilder();
        var query = $"SELECT * FROM \"{request.SchemaName}\".\"{request.TableName}\"";

        try
        {
            await using var npgsqlConnection =
                new NpgsqlConnection(connectionService.GetConnectionString(connection));
            await npgsqlConnection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(query, npgsqlConnection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var headers = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetName)
                .Select(h => $"\"{h}\"");
            csv.AppendLine(string.Join(",", headers));

            while (await reader.ReadAsync(cancellationToken))
            {
                var values = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString();
                    values.Add($"\"{value?.Replace("\"", "\"\"")}\"");
                }

                csv.AppendLine(string.Join(",", values));
            }

            logger.LogInformation("Exported table {Schema}.{Table} to CSV", request.SchemaName, request.TableName);

            return csv.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exporting {Schema}.{Table} to CSV", request.SchemaName, request.TableName);
            return Error.Failure("Tables.ExportError", $"CSV export failed: {ex.Message}");
        }
    }
}