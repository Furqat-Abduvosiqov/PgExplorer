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

namespace Pg.Explorer.Features.Connections.UpdateConnection;

public sealed record UpdateConnectionRequest(
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string? Password);

public sealed record UpdateConnectionCommand(
    long Id,
    string? Name,
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string? Password)
    : IRequest<ErrorOr<ConnectionResponse>>;

internal sealed class UpdateConnectionCommandHandler(
    IConnectionRepository repository,
    IEncryptionService encryptionService,
    ILogger<UpdateConnectionCommandHandler> logger)
    : IRequestHandler<UpdateConnectionCommand, ErrorOr<ConnectionResponse>>
{
    public async Task<ErrorOr<ConnectionResponse>> Handle(UpdateConnectionCommand request,
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

public class UpdateConnectionEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("/api/connections/{id:long}", Handle)
            .WithTags("Connections");
    }

    private static async Task<IResult> Handle(
        [FromRoute] long id,
        [FromBody] UpdateConnectionRequest request,
        IValidator<UpdateConnectionRequest> validator,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var command = request.MapToCommand(id);

        var response = await mediator.Send(command, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}