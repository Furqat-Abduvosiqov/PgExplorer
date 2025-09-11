using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Infrastructure.Database;
using Pg.Explorer.Shared.Repositories;

namespace Pg.Explorer.Infrastructure.Repositories;

public class ConnectionRepository : BaseRepository<Connection, long>, IConnectionRepository
{
    public ConnectionRepository(PgExplorerDbContext context) : base(context)
    {
    }
}