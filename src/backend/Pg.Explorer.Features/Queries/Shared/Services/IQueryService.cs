using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Domain.Queries.Entities;

namespace Pg.Explorer.Features.Queries.Shared.Services;

public interface IQueryService
{
    Task<QueryEntity> ExecuteQueryAsync(QueryEntity query, ConnectionConfig connection,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<QueryHistory>> GetQueryHistoryAsync(int connectionId, int limit = 50);
    Task<bool> SaveQueryHistoryAsync(QueryHistory queryHistory);
    Task<bool> ClearQueryHistoryAsync(int connectionId);
}