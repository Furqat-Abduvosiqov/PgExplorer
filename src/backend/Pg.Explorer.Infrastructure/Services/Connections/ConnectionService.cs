using ErrorOr;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Features.Connections.TestNewConnection;
using Pg.Explorer.Shared.Encryptions;

namespace Pg.Explorer.Infrastructure.Services.Connections;

public class ConnectionService(
    ILogger<ConnectionService> logger,
    IEncryptionService encryptionService) : IConnectionService
{
    public async Task<ErrorOr<TestConnectionResponse>> TestExistingConnectionAsync(Connection connection)
    {
        try
        {
            var connectionString = GetConnectionString(connection);

            await using var npgsqlConnection = new NpgsqlConnection(connectionString);
            await npgsqlConnection.OpenAsync();

            logger.LogInformation(
                "Successfully tested connection to {Host}:{Port}/{Database}",
                connection.Host, connection.Port, connection.DatabaseName);

            return new TestConnectionResponse(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to connect to {Host}:{Port}/{Database}: {ErrorMessage}",
                connection.Host, connection.Port, connection.DatabaseName, ex.Message);

            return Error.Failure(
                code: "Connection.TestFailed",
                description: $"Could not connect to database: {ex.Message}");
        }
    }

    public async Task<ErrorOr<TestConnectionResponse>> TestNewConnectionAsync(TestNewConnectionCommand command)
    {
        try
        {
            var connectionString = GetConnectionString(command);

            await using var npgsqlConnection = new NpgsqlConnection(connectionString);
            await npgsqlConnection.OpenAsync();

            logger.LogInformation(
                "Successfully tested connection to {Host}:{Port}/{Database}",
                command.Host, command.Port, command.DatabaseName);

            return new TestConnectionResponse(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to connect to {Host}:{Port}/{Database}: {ErrorMessage}",
                command.Host, command.Port, command.DatabaseName, ex.Message);

            return Error.Failure(
                code: "Connection.TestFailed",
                description: $"Could not connect to database: {ex.Message}");
        }
    }

    public string GetConnectionString(Connection connection)
    {
        var decryptedPassword = encryptionService.DecryptPassword(connection.EncryptedPassword);

        var connectionString =
            $"Host={connection.Host};Port={connection.Port};Database={connection.DatabaseName};" +
            $"Username={connection.Username};Password={decryptedPassword};Timeout=30;";

        return connectionString;
    }

    private string GetConnectionString(TestNewConnectionCommand command)
    {
        var connectionString =
            $"Host={command.Host};Port={command.Port};Database={command.DatabaseName};" +
            $"Username={command.Username};Password={command.Password};Timeout=30;";

        return connectionString;
    }
}