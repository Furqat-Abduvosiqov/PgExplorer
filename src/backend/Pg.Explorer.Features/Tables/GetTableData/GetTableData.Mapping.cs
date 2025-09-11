namespace Pg.Explorer.Features.Tables.GetTableData;

public static class GetTableDataMappings
{
    public static GetTableDataCommand MapToCommand(this GetTableDataRequest request)
        => new(
            request.ConnectionId,
            request.SchemaName,
            request.TableName,
            request.Page,
            request.PageSize);
}