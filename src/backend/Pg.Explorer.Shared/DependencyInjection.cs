using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pg.Explorer.Shared.Encryptions;
using Pg.Explorer.Shared.Middlewares;

namespace Pg.Explorer.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddTransient<GlobalExceptionHandlingMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseUtilities(this IApplicationBuilder app)
    {
        app.UseCors("AllowAngularApp");
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        return app;
    }
}