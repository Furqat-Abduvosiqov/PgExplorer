using ErrorOr;
using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Features.Connections.TestNewConnection;

namespace Pg.Explorer.Features.Connections.Shared;

public interface IConnectionService
{
    Task<ErrorOr<TestConnectionResponse>> TestExistingConnectionAsync(Connection connection);
    Task<ErrorOr<TestConnectionResponse>> TestNewConnectionAsync(TestNewConnectionCommand command);
    string GetConnectionString(Connection connection);
}