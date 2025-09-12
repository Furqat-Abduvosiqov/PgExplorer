using ErrorOr;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Shared.Encryptions;

namespace Pg.Explorer.Infrastructure.Services.Connections;

public class ConnectionService : IConnectionService
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<ConnectionService> _logger;

    public ConnectionService(ILogger<ConnectionService> logger,
        IEncryptionService encryptionService)
    {
        _logger = logger;
        _encryptionService = encryptionService;
    }

    public async Task<ErrorOr<bool>> TestConnectionAsync(Connection connection)
    {
        try
        {
            var connectionString = GetConnectionString(connection);

            await using var npgsqlConnection = new NpgsqlConnection(connectionString);
            await npgsqlConnection.OpenAsync();

            _logger.LogInformation(
                "Successfully tested connection to {Host}:{Port}/{Database}",
                connection.Host, connection.Port, connection.DatabaseName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to connect to {Host}:{Port}/{Database}: {ErrorMessage}",
                connection.Host, connection.Port, connection.DatabaseName, ex.Message);

            return Error.Failure(
                code: "Connection.TestFailed",
                description: $"Could not connect to database: {ex.Message}");
        }
    }

    public string GetConnectionString(Connection connection)
    {
        var decryptedPassword = _encryptionService.DecryptPassword(connection.EncryptedPassword);

        var connectionString =
            $"Host={connection.Host};Port={connection.Port};Database={connection.DatabaseName};" +
            $"Username={connection.Username};Password={decryptedPassword};Timeout=30;";

        return connectionString;
    }
}