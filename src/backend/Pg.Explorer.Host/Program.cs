using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Pg.Explorer.Features;
using Pg.Explorer.Infrastructure;
using Pg.Explorer.Infrastructure.Seeding;
using Pg.Explorer.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "PostgreSQL Manager API",
        Version = "v1",
        Description = "Web API for PostgreSQL database management"
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(opts => { opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddUtilities();
builder.Services.AddFeatures();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseUtilities();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seeder.SeedAsync();
}

app.MapFeatures();
app.Run();