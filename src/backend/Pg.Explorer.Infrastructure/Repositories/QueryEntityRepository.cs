using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Features.Queries.Shared;
using Pg.Explorer.Infrastructure.Database;

namespace Pg.Explorer.Infrastructure.Repositories;

public class QueryEntityRepository : BaseRepository<QueryEntity, Guid>, IQueryEntityRepository
{
    public QueryEntityRepository(PgExplorerDbContext context) : base(context)
    {
    }
}