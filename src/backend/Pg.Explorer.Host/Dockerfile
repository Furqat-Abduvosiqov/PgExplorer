# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution and restore
COPY PgExplorer.sln ./
COPY src/backend/Pg.Explorer.Shared/Pg.Explorer.Shared.csproj src/backend/Pg.Explorer.Shared/
COPY src/backend/Pg.Explorer.Features/Pg.Explorer.Features.csproj src/backend/Pg.Explorer.Features/
COPY src/backend/Pg.Explorer.Domain/Pg.Explorer.Domain.csproj src/backend/Pg.Explorer.Domain/
COPY src/backend/Pg.Explorer.Infrastructure/Pg.Explorer.Infrastructure.csproj src/backend/Pg.Explorer.Infrastructure/
COPY src/backend/Pg.Explorer.Host/Pg.Explorer.Host.csproj src/backend/Pg.Explorer.Host/
RUN dotnet restore src/backend/Pg.Explorer.Host/Pg.Explorer.Host.csproj -r linux-x64

# copy sources and publish
COPY src/backend/Pg.Explorer.Shared/ src/backend/Pg.Explorer.Shared/
COPY src/backend/Pg.Explorer.Domain/ src/backend/Pg.Explorer.Domain/
COPY src/backend/Pg.Explorer.Infrastructure/ src/backend/Pg.Explorer.Infrastructure/
COPY src/backend/Pg.Explorer.Host/ src/backend/Pg.Explorer.Host/
COPY src/backend/Pg.Explorer.Features/ src/backend/Pg.Explorer.Features/
RUN dotnet publish src/backend/Pg.Explorer.Host/Pg.Explorer.Host.csproj -c Release -o /app/publish -r linux-x64 --no-restore --self-contained false

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# add entrypoint
COPY src/backend/Pg.Explorer.Host/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["/app/entrypoint.sh"]
