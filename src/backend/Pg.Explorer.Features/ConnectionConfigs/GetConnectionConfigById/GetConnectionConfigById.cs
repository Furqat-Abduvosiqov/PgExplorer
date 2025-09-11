using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Features.ConnectionConfigs.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.ConnectionConfigs.GetConnectionConfigById;

public sealed record GetConnectionConfigByIdQuery(long Id) : IRequest<ErrorOr<ConnectionConfigResponse>>;

internal sealed class GetConnectionConfigByIdQueryHandler(IConnectionConfigRepository repository)
    : IRequestHandler<GetConnectionConfigByIdQuery, ErrorOr<ConnectionConfigResponse>>
{
    public async Task<ErrorOr<ConnectionConfigResponse>> Handle(GetConnectionConfigByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Error.NotFound(code: "ConnectionConfig.NotFound",
                description: $"Connection config {request.Id} not found.");
        }

        var response = entity.MapToResponse();
        return response;
    }
}

public class GetConnectionConfigByIdEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/connection-configs/{id:long}", Handle)
            .WithTags("ConnectionConfigs");
    }

    private static async Task<IResult> Handle(
        [FromRoute] long id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetConnectionConfigByIdQuery(id), cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}