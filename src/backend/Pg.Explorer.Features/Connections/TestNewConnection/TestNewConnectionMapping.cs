namespace Pg.Explorer.Features.Connections.TestNewConnection;

public static class TestNewConnectionMapping
{
    public static TestNewConnectionCommand MapToCommand(this TestNewConnectionRequest request) =>
        new(
            request.Host,
            request.Port,
            request.DatabaseName,
            request.Username,
            request.Password);
}