namespace Pg.Explorer.Features.Tables.Shared;

public class ExecutionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Dictionary<string, object?>> Data { get; set; } = new();
    public List<string> Columns { get; set; } = new();
    public int RowsAffected { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string QueryText { get; set; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; set; }
}