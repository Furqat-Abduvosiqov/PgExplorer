namespace Pg.Explorer.Features.ConnectionConfigs.UpdateConnectionConfig;

public static class UpdateConnectionConfigMapping
{
    public static UpdateConnectionConfigCommand MapToCommand(this UpdateConnectionConfigRequest request, long id)
        => new(
            id,
            request.Name,
            request.Host,
            request.Port,
            request.DatabaseName,
            request.Username,
            request.Password);
}