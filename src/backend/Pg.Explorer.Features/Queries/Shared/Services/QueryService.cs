using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Features.Tables.Shared;
using Pg.Explorer.Shared.Encryptions;

namespace Pg.Explorer.Features.Queries.Shared.Services;

public class QueryService : IQueryService
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<QueryService> _logger;

    public QueryService(ILogger<QueryService> logger, IEncryptionService encryptionService)
    {
        _logger = logger;
        _encryptionService = encryptionService;
    }

    public async Task<QueryEntity> ExecuteQueryAsync(
        QueryEntity query,
        Connection connection,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var connectionString = GetConnectionString(connection);

            await using var npgsqlConnection = new NpgsqlConnection(connectionString);
            await npgsqlConnection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(query.QueryBody, npgsqlConnection);
            command.CommandTimeout = 30;


            switch (query.QueryType)
            {
                case QueryType.Read:
                {
                    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                    var count = 0;
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        count++;
                    }

                    query.AffectedRows = count;
                    break;
                }

                case QueryType.Write:
                case QueryType.Delete:
                case QueryType.Update:
                {
                    query.AffectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
                    break;
                }

                default:
                    throw new NotSupportedException($"Unsupported query type: {query.QueryType}");
            }


            stopwatch.Stop();

            query.ExecutionTime = stopwatch.Elapsed;
            query.QueryStatus = QueryStatus.Success;
            query.ExecutedAt = DateTimeOffset.UtcNow;
            query.ErrorMessage = null;

            _logger.LogInformation(
                "Query executed successfully for connection {ConnectionId}. Execution time: {ExecutionTime}ms",
                connection.Id, query.ExecutionTime.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            query.ExecutionTime = stopwatch.Elapsed;
            query.QueryStatus = QueryStatus.Error;
            query.ErrorMessage = ex.Message;
            query.ExecutedAt = DateTimeOffset.UtcNow;

            _logger.LogError(ex, "Query execution failed for connection {ConnectionId}", connection.Id);
        }

        return query;
    }

    public async Task<ExecutionResult> ExecuteQueryWithResultAsync(QueryEntity query, Connection connection,
        CancellationToken cancellationToken = default)
    {
        var result = new ExecutionResult
        {
            QueryText = query.QueryBody,
            ExecutedAt = DateTimeOffset.UtcNow,
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var connectionString = GetConnectionString(connection);

            await using var npgsqlConnection = new NpgsqlConnection(connectionString);
            await npgsqlConnection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(query.QueryBody, npgsqlConnection);
            command.CommandTimeout = 30;

            if (query.QueryType == QueryType.Read)
            {
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                // Columns
                for (int i = 0; i < reader.FieldCount; i++)
                    result.Columns.Add(reader.GetName(i));

                // Rows
                while (await reader.ReadAsync(cancellationToken))
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        row[result.Columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    result.Data.Add(row);
                }

                result.RowsAffected = result.Data.Count;
            }
            else
            {
                result.RowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            }

            stopwatch.Stop();

            result.ExecutionTime = stopwatch.Elapsed;
            result.Success = true;
            result.Message = $"Executed successfully ({result.RowsAffected} rows).";

            query.ExecutionTime = result.ExecutionTime;
            query.QueryStatus = QueryStatus.Success;
            query.ExecutedAt = result.ExecutedAt;
            query.AffectedRows = result.RowsAffected;
            query.ErrorMessage = null;

            _logger.LogInformation(
                "Query executed successfully for connection {ConnectionId}. Execution time: {ExecutionTime}ms",
                connection.Id, query.ExecutionTime.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            result.ExecutionTime = stopwatch.Elapsed;
            result.Success = false;
            result.Message = ex.Message;

            query.ExecutionTime = result.ExecutionTime;
            query.QueryStatus = QueryStatus.Error;
            query.ExecutedAt = DateTimeOffset.UtcNow;
            query.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Query execution failed for connection {ConnectionId}", connection.Id);
        }

        return result;
    }

    private string GetConnectionString(Connection connection)
    {
        var decryptedPassword = _encryptionService.DecryptPassword(connection.EncryptedPassword);

        var connectionString =
            $"Host={connection.Host};Port={connection.Port};Database={connection.DatabaseName};" +
            $"Username={connection.Username};Password={decryptedPassword};Timeout=30;";

        return connectionString;
    }
}