namespace Pg.Explorer.Features.Tables.GetTableData;

public static class GetTableDataMappings
{
    public static GetTableDataCommand MapToCommand(this GetTableDataQuery query)
        => new(
            query.ConnectionId,
            query.SchemaName,
            query.TableName,
            query.Page,
            query.PageSize);
}