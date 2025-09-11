namespace Pg.Explorer.Features.Tables.ExportTable;

public static class ExportTableMapping
{
    public static ExportTableCommand MapToCommand(this ExportTableRequest request) =>
        new(request.ConnectionId, request.SchemaName, request.TableName);
}