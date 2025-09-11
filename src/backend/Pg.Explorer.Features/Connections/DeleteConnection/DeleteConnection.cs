using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Connections.DeleteConnection;

internal sealed record DeleteConnectionCommand(long Id) : IRequest<ErrorOr<Unit>>;

internal sealed class DeleteConnectionCommandHandler(
    IConnectionRepository repository,
    ILogger<DeleteConnectionCommandHandler> logger)
    : IRequestHandler<DeleteConnectionCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(DeleteConnectionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Error.NotFound(code: "Connection.NotFound",
                description: $"Connection with id {request.Id} not found.");
        }

        repository.Remove(entity);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted connection: {@ConnectionId}", request.Id);
        return Unit.Value;
    }
}

public class DeleteConnectionEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/connections/{id:long}", Handle)
            .WithTags("Connections");
    }

    private static async Task<IResult> Handle(
        [FromRoute] long id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Results.BadRequest("Invalid id.");

        var response = await mediator.Send(new DeleteConnectionCommand(id), cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.NoContent();
    }
}