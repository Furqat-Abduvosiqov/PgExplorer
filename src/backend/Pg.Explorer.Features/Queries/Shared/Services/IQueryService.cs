using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Features.Tables.Shared;

namespace Pg.Explorer.Features.Queries.Shared.Services;

public interface IQueryService
{
    Task<QueryEntity> ExecuteQueryAsync(QueryEntity query, Connection connection,
        CancellationToken cancellationToken = default);

    Task<ExecutionResult> ExecuteQueryWithResultAsync(QueryEntity query, Connection connection,
        CancellationToken cancellationToken = default);
}