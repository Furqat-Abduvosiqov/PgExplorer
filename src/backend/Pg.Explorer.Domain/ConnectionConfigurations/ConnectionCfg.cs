using Pg.Explorer.Domain.Queries.Entities;

namespace Pg.Explorer.Domain.ConnectionConfigurations;

public class ConnectionCfg
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public required string Host { get; set; }
    public int Port { get; set; }
    public string DatabaseName { get; set; } = "postgres";
    public required string Username { get; set; }
    public required string EncryptedPassword { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public IEnumerable<Query> Queries { get; set; } = Enumerable.Empty<Query>();
}