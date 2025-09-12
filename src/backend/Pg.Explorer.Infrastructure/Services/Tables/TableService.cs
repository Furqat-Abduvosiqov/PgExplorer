using System.Globalization;
using System.Text;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Features.Queries.Shared;
using Pg.Explorer.Features.Tables.DeleteRow;
using Pg.Explorer.Features.Tables.ExportTable;
using Pg.Explorer.Features.Tables.GetTableData;
using Pg.Explorer.Features.Tables.InsertRow;
using Pg.Explorer.Features.Tables.Shared;
using Pg.Explorer.Features.Tables.UpdateRow;

namespace Pg.Explorer.Infrastructure.Services.Tables;

public class TableService(
    IQueryEntityRepository queryRepository,
    IQueryService queryService,
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

        var limit = request.PageSize;
        var offset = (request.Page - 1) * request.PageSize;

        var queryText = $@"
        SELECT * 
        FROM ""{request.SchemaName}"".""{request.TableName}""
        LIMIT {limit} OFFSET {offset};";

        var query = QueryEntity.Create(connection.Id, queryText, QueryType.Read);

        var executionResult = await queryService.ExecuteQueryWithResultAsync(query, connection, cancellationToken);

        await queryRepository.AddAsync(query, cancellationToken);
        await queryRepository.SaveChangesAsync(cancellationToken);

        return executionResult.Success
            ? executionResult
            : Error.Failure("Tables.QueryError", $"Failed to retrieve table data: {executionResult.Message}");
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
        var values = string.Join(", ", request.RowData.Values.Select(ToSqlLiteral));

        var sql = $"INSERT INTO \"{request.SchemaName}\".\"{request.TableName}\" ({columns}) VALUES ({values});";

        var query = QueryEntity.Create(connection.Id, sql, QueryType.Write);

        var result = await queryService.ExecuteQueryWithResultAsync(query, connection, cancellationToken);

        await queryRepository.AddAsync(query, cancellationToken);
        await queryRepository.SaveChangesAsync(cancellationToken);

        return result.Success
            ? result
            : Error.Failure("Tables.InsertError", $"Insert failed: {result.Message}");
    }

    public async Task<ErrorOr<ExecutionResult>> UpdateRowAsync(
        UpdateRowCommand request,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound("ConnectionConfig.NotFound",
                $"Connection config {request.ConnectionId} not found.");

        var setClause = string.Join(", ",
            request.RowData.Select(kvp => $"\"{kvp.Key}\" = {ToSqlLiteral(kvp.Value)}"));

        var whereClause = string.Join(" AND ",
            request.WhereConditions.Select(kvp => $"\"{kvp.Key}\" = {ToSqlLiteral(kvp.Value)}"));

        var sql = $"UPDATE \"{request.SchemaName}\".\"{request.TableName}\" SET {setClause} WHERE {whereClause};";

        var query = QueryEntity.Create(connection.Id, sql, QueryType.Update);

        var result = await queryService.ExecuteQueryWithResultAsync(query, connection, cancellationToken);

        await queryRepository.AddAsync(query, cancellationToken);
        await queryRepository.SaveChangesAsync(cancellationToken);

        return result.Success
            ? result
            : Error.Failure("Tables.UpdateError", $"Update failed: {result.Message}");
    }

    public async Task<ErrorOr<ExecutionResult>> DeleteRowAsync(
        DeleteRowCommand request,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
            return Error.NotFound("ConnectionConfig.NotFound",
                $"Connection config {request.ConnectionId} not found.");

        var whereClause = string.Join(" AND ",
            request.WhereConditions.Select(kvp => $"\"{kvp.Key}\" = {ToSqlLiteral(kvp.Value)}"));

        var sql = $"DELETE FROM \"{request.SchemaName}\".\"{request.TableName}\" WHERE {whereClause};";

        var query = QueryEntity.Create(connection.Id, sql, QueryType.Delete);

        var result = await queryService.ExecuteQueryWithResultAsync(query, connection, cancellationToken);

        await queryRepository.AddAsync(query, cancellationToken);
        await queryRepository.SaveChangesAsync(cancellationToken);

        return result.Success
            ? result
            : Error.Failure("Tables.DeleteError", $"Delete failed: {result.Message}");
    }

    public async Task<ErrorOr<string>> ExportTableToCsvAsync(
        ExportTableCommand request,
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

    private static string ToSqlLiteral(object? value)
    {
        if (value is null) return "NULL";

        return value switch
        {
            string s => $"'{s.Replace("'", "''")}'", // escape single quotes
            bool b => b ? "TRUE" : "FALSE",
            int or long or short or byte or decimal or double or float => Convert.ToString(value,
                CultureInfo.InvariantCulture)!,
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            _ => $"'{value.ToString()!.Replace("'", "''")}'"
        };
    }
}