using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Infrastructure.Database;
using Pg.Explorer.Shared.Repositories;

namespace Pg.Explorer.Infrastructure.Repositories;

public class ConnectionConfigRepository : BaseRepository<ConnectionConfig>, IConnectionConfigRepository
{
    public ConnectionConfigRepository(PgExplorerDbContext context) : base(context)
    {
    }
}