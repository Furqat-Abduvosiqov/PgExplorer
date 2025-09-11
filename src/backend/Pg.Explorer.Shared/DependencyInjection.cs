using Microsoft.Extensions.DependencyInjection;
using Pg.Explorer.Shared.Encryptions;
using Pg.Explorer.Shared.Middlewares;

namespace Pg.Explorer.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddTransient<GlobalExceptionHandlingMiddleware>();
        return services;
    }
}