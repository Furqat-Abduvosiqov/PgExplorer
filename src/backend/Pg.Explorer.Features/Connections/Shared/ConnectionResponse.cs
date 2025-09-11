namespace Pg.Explorer.Features.Connections.Shared;

public sealed record ConnectionResponse(
    long Id,
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);