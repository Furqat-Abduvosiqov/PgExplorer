namespace Pg.Explorer.Features.Tables.Shared;

public class IndexInfo
{
    public string IndexName { get; init; } = string.Empty;
    public string IndexType { get; init; } = string.Empty;
    public bool IsUnique { get; init; }
    public bool IsPrimary { get; init; }
    public List<string> Columns { get; init; } = [];
}