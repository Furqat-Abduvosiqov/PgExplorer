namespace Pg.Explorer.Features.Tables.InsertRow;

public static class InsertRowMappings
{
    public static InsertRowCommand MappToCommand(this InsertRowRequest request) =>
        new(
            request.ConnectionId,
            request.SchemaName,
            request.TableName,
            request.RowData);
}