using Pg.Explorer.Shared.Repositories.interfaces;

namespace Pg.Explorer.Domain.Connections;

public interface IConnectionRepository : IRepository<Connection, long>
{
}