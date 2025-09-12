using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Features.Databases.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Databases.DropDatabase;

internal sealed record DropDatabaseCommand(
    long ConnectionId,
    string DatabaseName) : IRequest<ErrorOr<bool>>;

internal sealed class DropDatabaseCommandHandler(
    IDatabaseService databaseService,
    ILogger<DropDatabaseCommandHandler> logger)
    : IRequestHandler<DropDatabaseCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(DropDatabaseCommand request, CancellationToken cancellationToken)
    {
        var result = await databaseService.DropDatabaseAsync(
            request.ConnectionId,
            request.DatabaseName,
            cancellationToken);

        if (result.IsError)
        {
            logger.LogWarning("Failed to drop database {DatabaseName} on connection {ConnectionId}",
                request.DatabaseName, request.ConnectionId);

            return result.FirstError;
        }

        logger.LogInformation("Database {DatabaseName} dropped successfully on connection {ConnectionId}",
            request.DatabaseName, request.ConnectionId);

        return true;
    }
}

public class DropDatabaseEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/databases/{databaseName}", Handle)
            .WithTags("Databases");
    }

    private static async Task<IResult> Handle(
        [FromRoute] string databaseName,
        [FromQuery] long connectionId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (connectionId <= 0)
            return Results.BadRequest("ConnectionId is required");

        if (string.IsNullOrWhiteSpace(databaseName))
            return Results.BadRequest("DatabaseName is required");

        var command = new DropDatabaseCommand(connectionId, databaseName);

        var response = await mediator.Send(command, cancellationToken);

        if (response.IsError)
            return response.Errors.ToProblem();

        return Results.Ok(new { success = response.Value });
    }
}