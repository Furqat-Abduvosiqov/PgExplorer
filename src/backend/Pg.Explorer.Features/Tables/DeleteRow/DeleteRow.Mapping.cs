namespace Pg.Explorer.Features.Tables.DeleteRow;

public static class DeleteRowMapping
{
    public static DeleteRowCommand MapToCommand(this DeleteRowRequest request) =>
        new DeleteRowCommand(
            request.ConnectionId,
            request.SchemaName,
            request.TableName,
            request.WhereConditions);
}