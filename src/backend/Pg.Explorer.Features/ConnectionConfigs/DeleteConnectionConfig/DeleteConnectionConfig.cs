using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.ConnectionConfigs.DeleteConnectionConfig;

internal sealed record DeleteConnectionConfigCommand(long Id) : IRequest<ErrorOr<Unit>>;

internal sealed class DeleteConnectionConfigCommandHandler(
    IConnectionConfigRepository repository,
    ILogger<DeleteConnectionConfigCommandHandler> logger)
    : IRequestHandler<DeleteConnectionConfigCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(DeleteConnectionConfigCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Error.NotFound(code: "ConnectionConfig.NotFound",
                description: $"Connection config {request.Id} not found.");
        }

        repository.Remove(entity);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted connection: {@ConnectionId}", request.Id);
        return Unit.Value;
    }
}

public class DeleteConnectionConfigEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/connection-configs/{id:long}", Handle)
            .WithTags("ConnectionConfigs");
    }

    private static async Task<IResult> Handle(
        [FromRoute] long id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new DeleteConnectionConfigCommand(id), cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.NoContent();
    }
}