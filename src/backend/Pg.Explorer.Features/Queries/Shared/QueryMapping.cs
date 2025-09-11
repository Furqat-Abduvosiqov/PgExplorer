using Pg.Explorer.Domain.Queries.Entities;

namespace Pg.Explorer.Features.Queries.Shared;

public static class QueryMapping
{
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