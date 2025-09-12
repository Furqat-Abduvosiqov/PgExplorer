using Pg.Explorer.Domain.Queries;

namespace Pg.Explorer.Features.Queries.Shared;

public interface IQueryEntityRepository : IRepository<QueryEntity, Guid>
{
}