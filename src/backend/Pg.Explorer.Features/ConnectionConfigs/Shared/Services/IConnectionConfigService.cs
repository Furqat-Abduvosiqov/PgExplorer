using ErrorOr;
using Pg.Explorer.Domain.ConnectionConfigurations;

namespace Pg.Explorer.Features.ConnectionConfigs.Shared.Services;

public interface IConnectionConfigService
{
    Task<ErrorOr<bool>> TestConnectionAsync(ConnectionConfig connection);
    string GetConnectionString(ConnectionConfig connection);
}