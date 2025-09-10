using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Shared.Repositories.interfaces;

namespace Pg.Explorer.Domain.Queries;

public interface IQueryEntityRepository : IRepository<QueryEntity, Guid>
{
}