using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Features.Databases.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Databases.CreateDatabase;

public sealed record CreateDatabaseRequest(
    long ConnectionId,
    string DatabaseName);

internal sealed record CreateDatabaseCommand(
    long ConnectionId,
    string DatabaseName) : IRequest<ErrorOr<bool>>;

internal sealed class CreateDatabaseCommandHandler(
    IDatabaseService databaseService,
    ILogger<CreateDatabaseCommandHandler> logger)
    : IRequestHandler<CreateDatabaseCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(CreateDatabaseCommand request, CancellationToken cancellationToken)
    {
        var result = await databaseService.CreateDatabaseAsync(
            request.ConnectionId,
            request.DatabaseName,
            cancellationToken);

        if (result.IsError)
        {
            logger.LogWarning("Failed to create database {DatabaseName} on connection {ConnectionId}",
                request.DatabaseName, request.ConnectionId);

            return result.FirstError;
        }

        logger.LogInformation("Database {DatabaseName} created successfully on connection {ConnectionId}",
            request.DatabaseName, request.ConnectionId);

        return true;
    }
}

public class CreateDatabaseEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/databases", Handle)
            .WithTags("Databases");
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateDatabaseRequest request,
        IValidator<CreateDatabaseRequest> validator,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var command = new CreateDatabaseCommand(request.ConnectionId, request.DatabaseName);

        var response = await mediator.Send(command, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(new { success = response.Value });
    }
}