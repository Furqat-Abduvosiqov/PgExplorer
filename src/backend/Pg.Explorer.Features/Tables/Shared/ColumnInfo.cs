namespace Pg.Explorer.Features.Tables.Shared;

public class ColumnInfo
{
    public string ColumnName { get; init; } = null!;
    public string DataType { get; init; } = null!;
    public string? DefaultValue { get; set; }
    public bool IsNullable { get; init; }
    public bool IsPrimaryKey { get; init; }
    public bool IsForeignKey { get; init; }
    public int? MaxLength { get; init; }
    public int OrdinalPosition { get; init; }
}