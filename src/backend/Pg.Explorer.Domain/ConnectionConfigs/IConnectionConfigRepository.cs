using Pg.Explorer.Shared.Repositories.interfaces;

namespace Pg.Explorer.Domain.ConnectionConfigs;

public interface IConnectionConfigRepository : IRepository<ConnectionConfig, long>
{
}