using ErrorOr;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Shared.Encryptions;

namespace Pg.Explorer.Features.ConnectionConfigs.Shared.Services;

public class ConnectionConfigService : IConnectionConfigService
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<ConnectionConfigService> _logger;

    public ConnectionConfigService(ILogger<ConnectionConfigService> logger,
        IEncryptionService encryptionService)
    {
        _logger = logger;
        _encryptionService = encryptionService;
    }

    public async Task<ErrorOr<bool>> TestConnectionAsync(ConnectionConfig connection)
    {
        try
        {
            var decryptedPassword = _encryptionService.DecryptPassword(connection.EncryptedPassword);
            var connectionString =
                $"Host={connection.Host};Port={connection.Port};Database={connection.DatabaseName};" +
                $"Username={connection.Username};Password={decryptedPassword};Timeout=30;";

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
}