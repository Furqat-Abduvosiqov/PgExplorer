using Pg.Explorer.Domain.Connections;

namespace Pg.Explorer.Features.Connections.CreateConnection;

internal static class CreateConnectionMappingExtension
{
    public static CreateConnectionCommand MapToCommand(this CreateConnectionRequest request)
        => new(
            request.Name,
            request.Host,
            request.Port,
            request.DatabaseName,
            request.Username,
            request.Password);

    public static Connection MapToConnectionConfig(this CreateConnectionCommand command,
        string encryptedPassword)
        => Connection.Create
        (
            command.Name,
            command.Host,
            command.Port,
            command.DatabaseName,
            command.Username,
            encryptedPassword
        );
}