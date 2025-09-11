using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Domain.Queries.Entities;

namespace Pg.Explorer.Features.Queries.Shared.Services;

public interface IQueryService
{
    Task<QueryEntity> ExecuteQueryAsync(QueryEntity query, Connection connection,
        CancellationToken cancellationToken = default);
}