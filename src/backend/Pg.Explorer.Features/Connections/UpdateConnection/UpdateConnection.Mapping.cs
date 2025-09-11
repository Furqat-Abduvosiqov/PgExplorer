namespace Pg.Explorer.Features.Connections.UpdateConnection;

public static class UpdateConnectionMapping
{
    public static UpdateConnectionCommand MapToCommand(this UpdateConnectionRequest request, long id)
        => new(
            id,
            request.Name,
            request.Host,
            request.Port,
            request.DatabaseName,
            request.Username,
            request.Password);
}