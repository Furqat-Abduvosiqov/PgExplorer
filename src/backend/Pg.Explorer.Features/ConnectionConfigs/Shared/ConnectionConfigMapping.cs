using Pg.Explorer.Domain.ConnectionConfigurations;

namespace Pg.Explorer.Features.ConnectionConfigs.Shared;

public static class ConnectionConfigMapping
{
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