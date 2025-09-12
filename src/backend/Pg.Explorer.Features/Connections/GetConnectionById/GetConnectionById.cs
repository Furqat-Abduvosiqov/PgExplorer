using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Connections.GetConnectionById;

public sealed record GetConnectionByIdQuery(long Id) : IRequest<ErrorOr<ConnectionResponse>>;

internal sealed class GetConnectionByIdQueryHandler(IConnectionRepository repository)
    : IRequestHandler<GetConnectionByIdQuery, ErrorOr<ConnectionResponse>>
{
    public async Task<ErrorOr<ConnectionResponse>> Handle(GetConnectionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Error.NotFound(code: "Connection.NotFound",
                description: $"Connection with id {request.Id} not found.");
        }

        var response = entity.MapToResponse();
        return response;
    }
}

public class GetConnectionByIdEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/connections/{id:long}", Handle)
            .WithTags("Connections");
    }

    private static async Task<IResult> Handle(
        [FromRoute] long id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetConnectionByIdQuery(id), cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}