using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.ConnectionConfigs;
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

    private string GetConnectionString(ConnectionConfig connection)
    {
        var decryptedPassword = _encryptionService.DecryptPassword(connection.EncryptedPassword);

        var connectionString =
            $"Host={connection.Host};Port={connection.Port};Database={connection.DatabaseName};" +
            $"Username={connection.Username};Password={decryptedPassword};Timeout=30;";

        return connectionString;
    }
}