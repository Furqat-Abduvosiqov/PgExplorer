using System.Diagnostics.CodeAnalysis;
using Pg.Explorer.Domain.ConnectionConfigs;

namespace Pg.Explorer.Domain.Queries.Entities;

/// <summary>
/// 
/// </summary>
[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class QueryEntity
{
    private QueryEntity()
    {
    }

    public Guid Id { get; set; }
    public long ConnectionId { get; set; }

    public virtual ConnectionConfig? Connection { get; set; }
    public required string QueryBody { get; set; }
    public QueryType QueryType { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public QueryStatus QueryStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public int AffectedRows { get; set; }

    public static QueryEntity Create(long connectionId, string queryBody, QueryType queryType) =>
        new()
        {
            ConnectionId = connectionId,
            QueryBody = queryBody,
            QueryType = queryType
        };
}