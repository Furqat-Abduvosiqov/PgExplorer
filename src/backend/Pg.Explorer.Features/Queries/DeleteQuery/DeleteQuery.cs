using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Queries.DeleteQuery;

internal sealed record DeleteQueryCommand(Guid Id) : IRequest<ErrorOr<Unit>>;

internal sealed class DeleteQueryCommandHandler(
    IQueryEntityRepository repository,
    ILogger<DeleteQueryCommandHandler> logger)
    : IRequestHandler<DeleteQueryCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(DeleteQueryCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Error.NotFound("Query.NotFound", $"Query with id {request.Id} not found.");
        }

        repository.Remove(entity);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted query: {@QueryId}", request.Id);
        return Unit.Value;
    }
}

public class DeleteQueryEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/queries/{id:guid}", Handle)
            .WithTags("Queries");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new DeleteQueryCommand(id), cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.NoContent();
    }
}