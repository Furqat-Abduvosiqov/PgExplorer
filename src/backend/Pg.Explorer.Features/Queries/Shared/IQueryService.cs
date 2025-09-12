using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Features.Tables.Shared;

namespace Pg.Explorer.Features.Queries.Shared;

public interface IQueryService
{
    Task<QueryEntity> ExecuteQueryAsync(QueryEntity query, Connection connection,
        CancellationToken cancellationToken = default);

    Task<ExecutionResult> ExecuteQueryWithResultAsync(QueryEntity query, Connection connection,
        CancellationToken cancellationToken = default);
}