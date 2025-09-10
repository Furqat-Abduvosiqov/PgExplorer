using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Features.Abstractions;
using Pg.Explorer.Features.ConnectionConfigs.Shared;
using Pg.Explorer.Features.Extensions;
using Pg.Explorer.Shared.Pagination;

namespace Pg.Explorer.Features.ConnectionConfigs.GetConnectionConfigList;

public sealed record GetConnectionConfigListQuery(string? Search, PageRequest Page)
    : IRequest<ErrorOr<PageResult<ConnectionConfigResponse>>>;

internal sealed class GetConnectionConfigListQueryHandler(IConnectionConfigRepository repository)
    : IRequestHandler<GetConnectionConfigListQuery, ErrorOr<PageResult<ConnectionConfigResponse>>>
{
    public async Task<ErrorOr<PageResult<ConnectionConfigResponse>>> Handle(GetConnectionConfigListQuery request,
        CancellationToken cancellationToken)
    {
        var all = await repository.GetAllAsync(cancellationToken);
        var query = all.AsQueryable()
            .WhereIf(!string.IsNullOrWhiteSpace(request.Search), x =>
                (x.Name ?? string.Empty).Contains(request.Search!, StringComparison.OrdinalIgnoreCase)
                || x.Host.Contains(request.Search!, StringComparison.OrdinalIgnoreCase)
                || x.DatabaseName.Contains(request.Search!, StringComparison.OrdinalIgnoreCase)
                || x.Username.Contains(request.Search!, StringComparison.OrdinalIgnoreCase))
            .OrderByDynamic(request.Page.SortBy ?? nameof(ConnectionConfigResponse.UpdatedAt), desc: request.Page.Desc);

        var projected = query.Select(x =>
            new ConnectionConfigResponse(x.Id, x.Name, x.Host, x.Port, x.DatabaseName, x.Username, x.CreatedAt,
                x.UpdatedAt));
        var page = await projected.ToPageResultAsync(request.Page.PageNumber, request.Page.Size, cancellationToken);
        return page;
    }
}

public class GetConnectionConfigListEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/connection-configs", Handle)
            .WithTags("ConnectionConfigs");
    }

    private static async Task<IResult> Handle(
        IMediator mediator,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        CancellationToken cancellationToken = default)
    {
        var response =
            await mediator.Send(new GetConnectionConfigListQuery(search, new PageRequest(page, pageSize, sortBy, desc)),
                cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}