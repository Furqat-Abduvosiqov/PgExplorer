namespace Pg.Explorer.Features.Tables.UpdateRow;

public static class UpdateRowMapping
{
    public static UpdateRowCommand MapToCommand(this UpdateRowRequest request)
        => new(
            request.ConnectionId,
            request.SchemaName,
            request.TableName,
            request.RowData,
            request.WhereConditions);
}