using Microsoft.Extensions.DependencyInjection;
using Pg.Explorer.Shared.Encryptions;

namespace Pg.Explorer.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddScoped<IEncryptionService, EncryptionService>();
        return services;
    }
}