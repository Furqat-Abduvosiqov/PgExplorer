using System.Diagnostics.CodeAnalysis;
using Pg.Explorer.Domain.Queries.Entities;

namespace Pg.Explorer.Domain.ConnectionConfigs;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class ConnectionConfig
{
    private ConnectionConfig()
    {
    }

    public long Id { get; set; }
    public string? Name { get; set; }
    public required string Host { get; set; }
    public int Port { get; set; }
    public string DatabaseName { get; set; } = "postgres";
    public required string Username { get; set; }
    public required string EncryptedPassword { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public IEnumerable<QueryEntity> Queries { get; set; } = Enumerable.Empty<QueryEntity>();

    public static ConnectionConfig Create(string? name, string host, int port, string databaseName, string username,
        string password)
    {
        return new ConnectionConfig
        {
            Name = name,
            Host = host,
            Port = port,
            DatabaseName = databaseName,
            Username = username,
            EncryptedPassword = password,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}