using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Infrastructure.Database;

namespace Pg.Explorer.Infrastructure.Repositories;

public class ConnectionRepository : BaseRepository<Connection, long>, IConnectionRepository
{
    public ConnectionRepository(PgExplorerDbContext context) : base(context)
    {
    }
}