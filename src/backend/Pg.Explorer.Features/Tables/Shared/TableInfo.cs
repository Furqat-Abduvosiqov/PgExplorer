namespace Pg.Explorer.Features.Tables.Shared;

public class TableInfo
{
    public string SchemaName { get; init; } = string.Empty;
    public string TableName { get; init; } = string.Empty;
    public string TableType { get; init; } = string.Empty;
    public long RowCount { get; init; }
    public string Size { get; init; } = string.Empty;
    public List<ColumnInfo> Columns { get; init; } = [];
    public List<IndexInfo> Indexes { get; init; } = [];
}