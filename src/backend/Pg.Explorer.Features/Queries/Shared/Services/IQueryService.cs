using Pg.Explorer.Domain.ConnectionConfigs;
using Pg.Explorer.Domain.Queries.Entities;

namespace Pg.Explorer.Features.Queries.Shared.Services;

public interface IQueryService
{
    Task<QueryEntity> ExecuteQueryAsync(QueryEntity query, ConnectionConfig connection,
        CancellationToken cancellationToken = default);
}