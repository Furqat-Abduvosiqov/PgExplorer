using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Features.Abstractions;
using Pg.Explorer.Features.ConnectionConfigs.CreateConnectionConfig;
using Pg.Explorer.Features.ConnectionConfigs.Shared;
using Pg.Explorer.Features.Extensions;
using Pg.Explorer.Shared.Encryptions;

namespace Pg.Explorer.Features.ConnectionConfigs.UpdateConnectionConfig;

public sealed record UpdateConnectionConfigRequest(
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string? Password);

public sealed record UpdateConnectionConfigCommand(
    long Id,
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string? Password)
    : IRequest<ErrorOr<ConnectionConfigResponse>>;

internal sealed class UpdateConnectionConfigCommandHandler(
    IConnectionConfigRepository repository,
    IEncryptionService encryptionService,
    ILogger<UpdateConnectionConfigCommandHandler> logger)
    : IRequestHandler<UpdateConnectionConfigCommand, ErrorOr<ConnectionConfigResponse>>
{
    public async Task<ErrorOr<ConnectionConfigResponse>> Handle(UpdateConnectionConfigCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Error.NotFound(code: "ConnectionConfig.NotFound",
                description: $"Connection config {request.Id} not found.");
        }

        var encryptedPassword = request.Password is not null
            ? encryptionService.EncryptPassword(request.Password)
            : entity.EncryptedPassword;

        entity.Name = request.Name;
        entity.Host = request.Host;
        entity.Port = request.Port;
        entity.DatabaseName = request.DatabaseName;
        entity.Username = request.Username;
        entity.EncryptedPassword = encryptedPassword;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repository.Update(entity);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated connection: {@Connection}", entity);
        return entity.MapToResponse();
    }
}

public class UpdateConnectionConfigEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("/api/connection-configs/{id:long}", Handle)
            .WithTags("ConnectionConfigs");
    }

    private static async Task<IResult> Handle(
        [FromRoute] long id,
        [FromBody] UpdateConnectionConfigRequest request,
        IValidator<UpdateConnectionConfigRequest> validator,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var command = new UpdateConnectionConfigCommand(
            id,
            request.Name,
            request.Host,
            request.Port,
            request.DatabaseName,
            request.Username,
            request.Password);

        var response = await mediator.Send(command, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}