using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Features.Queries.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Queries.GetQueryById;

public sealed record GetQueryByIdQuery(Guid Id) : IRequest<ErrorOr<QueryResponse>>;

internal sealed class GetQueryByIdQueryHandler(IQueryEntityRepository repository)
    : IRequestHandler<GetQueryByIdQuery, ErrorOr<QueryResponse>>
{
    public async Task<ErrorOr<QueryResponse>> Handle(GetQueryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Error.NotFound("Query.NotFound", $"Query with id : {request.Id} not found.");
        }

        return entity.MapToResponse();
    }
}

public class GetQueryByIdEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/queries/{id:guid}", Handle)
            .WithTags("Queries");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetQueryByIdQuery(id), cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}