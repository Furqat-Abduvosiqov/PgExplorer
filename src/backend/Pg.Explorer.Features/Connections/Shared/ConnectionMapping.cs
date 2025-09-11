using Pg.Explorer.Domain.Connections;

namespace Pg.Explorer.Features.Connections.Shared;

public static class ConnectionMapping
{
    public static ConnectionResponse MapToResponse(this Connection connection)
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