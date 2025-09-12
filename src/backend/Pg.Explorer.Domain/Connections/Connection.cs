using System.Diagnostics.CodeAnalysis;
using Pg.Explorer.Domain.Queries;

namespace Pg.Explorer.Domain.Connections;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class Connection
{
    private Connection()
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

    public ICollection<QueryEntity> Queries { get; set; } = new List<QueryEntity>();

    public static Connection Create(string? name, string host, int port, string databaseName, string username,
        string password)
    {
        return new Connection
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

    public void TouchUpdatedAt() => UpdatedAt = DateTimeOffset.UtcNow;
}