using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Infrastructure.Database;
using Pg.Explorer.Shared.Repositories;

namespace Pg.Explorer.Infrastructure.Repositories;

public class QueryEntityRepository : BaseRepository<QueryEntity>, IQueryEntityRepository
{
    public QueryEntityRepository(PgExplorerDbContext context) : base(context)
    {
    }
}