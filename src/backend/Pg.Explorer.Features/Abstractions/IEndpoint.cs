using Microsoft.AspNetCore.Builder;

namespace Pg.Explorer.Features.Abstractions;

public interface IEndpoint
{
    void MapEndpoint(WebApplication app);
}