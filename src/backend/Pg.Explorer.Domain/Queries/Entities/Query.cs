using System.Text.Json;
using Pg.Explorer.Domain.ConnectionConfigurations;

namespace Pg.Explorer.Domain.Queries.Entities;

/// <summary>
/// 
/// </summary>
public class Query
{
    public Guid Id { get; set; }
    public long ConnectionId { get; set; }
    public virtual ConnectionCfg? ConnectionConfiguration { get; set; }
    public required string QueryBody { get; set; }
    public QueryType QueryType { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public QueryStatus QueryStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public int AffectedRows { get; set; }
    public JsonDocument? ExecutionResult { get; set; }
}