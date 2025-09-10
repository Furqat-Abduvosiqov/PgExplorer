namespace Pg.Explorer.Features.Application_Features.ConnectionConfigs.Shared;

public sealed record ConnectionConfigResponse(
    long Id,
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);