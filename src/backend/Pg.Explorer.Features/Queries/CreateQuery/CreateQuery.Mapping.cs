using Pg.Explorer.Domain.Queries.Entities;

namespace Pg.Explorer.Features.Queries.CreateQuery;

internal static class CreateQueryMapping
{
    public static CreateQueryCommand MapToCommand(this CreateQueryRequest request)
        => new(
            request.ConnectionId,
            request.QueryBody,
            request.QueryType);

    public static QueryEntity MapToQuery(this CreateQueryCommand request)
    {
        return QueryEntity.Create(request.ConnectionId, request.QueryBody, request.QueryType);
    }
}