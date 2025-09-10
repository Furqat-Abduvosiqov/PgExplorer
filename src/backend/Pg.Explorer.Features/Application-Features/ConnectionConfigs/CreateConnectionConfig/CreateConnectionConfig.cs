using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Features.Abstractions;
using Pg.Explorer.Features.Application_Features.ConnectionConfigs.Shared;
using Pg.Explorer.Features.Extensions;
using Pg.Explorer.Shared.Encryptions;

namespace Pg.Explorer.Features.Application_Features.ConnectionConfigs.CreateConnectionConfig;

public sealed record CreateConnectionConfigRequest(
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string Password);

internal sealed record CreateConnectionConfigCommand(
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string Password)
    : IRequest<ErrorOr<ConnectionConfigResponse>>;

internal sealed class CreateConnectionConfigCommandHandler(
    IConnectionConfigRepository repository,
    IEncryptionService encryptionService,
    ILogger<CreateConnectionConfigCommandHandler> logger)
    : IRequestHandler<CreateConnectionConfigCommand, ErrorOr<ConnectionConfigResponse>>
{
    public async Task<ErrorOr<ConnectionConfigResponse>> Handle(
        CreateConnectionConfigCommand request,
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

public class CreateConnectionConfigEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/connection-configs", Handle)
            .WithTags("ConnectionConfigs");
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateConnectionConfigRequest request,
        IValidator<CreateConnectionConfigRequest> validator,
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