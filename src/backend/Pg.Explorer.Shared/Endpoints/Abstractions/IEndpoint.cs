using Microsoft.AspNetCore.Builder;

namespace Pg.Explorer.Shared.Endpoints.Abstractions;

public interface IEndpoint
{
    void MapEndpoint(WebApplication app);
}