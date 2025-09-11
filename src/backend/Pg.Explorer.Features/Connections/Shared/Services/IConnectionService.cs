using ErrorOr;
using Pg.Explorer.Domain.Connections;

namespace Pg.Explorer.Features.Connections.Shared.Services;

public interface IConnectionService
{
    Task<ErrorOr<bool>> TestConnectionAsync(Connection connection);
    string GetConnectionString(Connection connection);
}