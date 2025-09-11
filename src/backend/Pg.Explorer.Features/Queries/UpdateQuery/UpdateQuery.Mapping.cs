using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Features.Queries.Shared;

namespace Pg.Explorer.Features.Queries.UpdateQuery;

internal static class UpdateQueryMapping
{
    public static UpdateQueryCommand MapToCommand(this UpdateQueryRequest request, Guid id)
        => new(
            id,
            request.ConnectionId,
            request.QueryBody,
            request.QueryType);

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