using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Features.ConnectionConfigs.Shared;

namespace Pg.Explorer.Features.ConnectionConfigs.CreateConnectionConfig;

internal static class CreateConnectionConfigMappingExtension
{
    public static CreateConnectionConfigCommand MapToCommand(this CreateConnectionConfigRequest request)
        => new(
            request.Name,
            request.Host,
            request.Port,
            request.DatabaseName,
            request.Username,
            request.Password);

    public static ConnectionConfig MapToConnectionConfig(this CreateConnectionConfigCommand command,
        string encryptedPassword)
        => ConnectionConfig.Create
        (
            command.Name,
            command.Host,
            command.Port,
            command.DatabaseName,
            command.Username,
            encryptedPassword
        );

    public static ConnectionConfigResponse MapToResponse(this ConnectionConfig connection)
        => new(
            connection.Id,
            connection.Name,
            connection.Host,
            connection.Port,
            connection.DatabaseName,
            connection.Username,
            connection.CreatedAt,
            connection.UpdatedAt);
}