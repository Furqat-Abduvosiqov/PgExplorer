using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Domain.Queries.Entities;
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
        ConnectionConfig connection,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var decryptedPassword = _encryptionService.DecryptPassword(connection.EncryptedPassword);
            var connectionString =
                $"Host={connection.Host};Port={connection.Port};Database={connection.DatabaseName};" +
                $"Username={connection.Username};Password={decryptedPassword};Timeout=30;";

            await using var npgsqlConnection = new NpgsqlConnection(connectionString);
            await npgsqlConnection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(query.QueryBody, npgsqlConnection);
            command.CommandTimeout = 30;


            var trimmedQuery = query.QueryBody.Trim().ToUpperInvariant();

            var isSelectQuery = trimmedQuery.StartsWith("SELECT") ||
                                trimmedQuery.StartsWith("WITH") ||
                                trimmedQuery.StartsWith("SHOW") ||
                                trimmedQuery.StartsWith("EXPLAIN");

            if (isSelectQuery)
            {
                var rows = new List<Dictionary<string, object?>>();

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                var columnNames = Enumerable.Range(0, reader.FieldCount)
                    .Select(reader.GetName)
                    .ToList();

                while (await reader.ReadAsync(cancellationToken))
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[columnNames[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }

                    rows.Add(row);
                }

                query.AffectedRows = rows.Count;
            }
            else
            {
                query.AffectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            }

            stopwatch.Stop();

            query.ExecutionTime = stopwatch.Elapsed;
            query.QueryStatus = QueryStatus.Success;
            query.ExecutedAt = DateTimeOffset.UtcNow;

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

    public Task<IEnumerable<QueryHistory>> GetQueryHistoryAsync(int connectionId, int limit = 50)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveQueryHistoryAsync(QueryHistory queryHistory)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ClearQueryHistoryAsync(int connectionId)
    {
        throw new NotImplementedException();
    }
}