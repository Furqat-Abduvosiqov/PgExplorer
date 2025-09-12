using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Shared.Encryptions;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Connections.CreateConnection;

public sealed record CreateConnectionRequest(
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string Password);

internal sealed record CreateConnectionCommand(
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string Password)
    : IRequest<ErrorOr<ConnectionResponse>>;

internal sealed class CreateConnectionCommandHandler(
    IConnectionRepository repository,
    IEncryptionService encryptionService,
    ILogger<CreateConnectionCommandHandler> logger)
    : IRequestHandler<CreateConnectionCommand, ErrorOr<ConnectionResponse>>
{
    public async Task<ErrorOr<ConnectionResponse>> Handle(
        CreateConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var encryptedPassword = encryptionService.EncryptPassword(request.Password);

        var connection = request.MapToConnectionConfig(encryptedPassword);

        await repository.AddAsync(connection, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created connection: {@Connection}", connection);

        var response = connection.MapToResponse();
        return response;
    }
}

public class CreateConnectionEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/connections", Handle)
            .WithTags("Connections");
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateConnectionRequest request,
        IValidator<CreateConnectionRequest> validator,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var command = request.MapToCommand();

        var response = await mediator.Send(command, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}