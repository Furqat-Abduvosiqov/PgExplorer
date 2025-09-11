using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Features.Queries.Shared;

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

    public static QueryResponse MapToResponse(this QueryEntity entity)
        => new(
            entity.Id,
            entity.ConnectionId,
            entity.QueryBody,
            entity.QueryType,
            entity.ExecutedAt,
            entity.ExecutionTime,
            entity.QueryStatus,
            entity.ErrorMessage,
            entity.AffectedRows);
}