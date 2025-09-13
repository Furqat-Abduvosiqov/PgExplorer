using ErrorOr;
using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Features.Connections.TestNewConnection;

namespace Pg.Explorer.Features.Connections.Shared;

public interface IConnectionService
{
    Task<ErrorOr<bool>> TestExistingConnectionAsync(Connection connection);
    Task<ErrorOr<bool>> TestNewConnectionAsync(TestNewConnectionCommand command);
    string GetConnectionString(Connection connection);
}