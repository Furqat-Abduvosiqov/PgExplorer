namespace Pg.Explorer.Features.Queries.UpdateQuery;

internal static class UpdateQueryMapping
{
    public static UpdateQueryCommand MapToCommand(this UpdateQueryRequest request, Guid id)
        => new(
            id,
            request.ConnectionId,
            request.QueryBody,
            request.QueryType);
}