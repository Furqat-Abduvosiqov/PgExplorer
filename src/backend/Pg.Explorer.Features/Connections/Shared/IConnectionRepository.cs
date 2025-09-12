using Pg.Explorer.Domain.Connections;

namespace Pg.Explorer.Features.Connections.Shared;

public interface IConnectionRepository : IRepository<Connection, long>
{
}