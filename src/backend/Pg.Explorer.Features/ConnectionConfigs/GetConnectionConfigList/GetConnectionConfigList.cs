using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pg.Explorer.Domain.ConnectionConfigs;
using Pg.Explorer.Features.ConnectionConfigs.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;
using Pg.Explorer.Shared.Pagination;

namespace Pg.Explorer.Features.ConnectionConfigs.GetConnectionConfigList;

public sealed record GetConnectionConfigListQuery(string? Search, PageRequest Page)
    : IRequest<ErrorOr<PageResult<ConnectionConfigResponse>>>;

internal sealed class GetConnectionConfigListQueryHandler(IConnectionConfigRepository repository)
    : IRequestHandler<GetConnectionConfigListQuery, ErrorOr<PageResult<ConnectionConfigResponse>>>
{
    public async Task<ErrorOr<PageResult<ConnectionConfigResponse>>> Handle(
        GetConnectionConfigListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Name ?? string.Empty, search) ||
                EF.Functions.Like(x.Host, search) ||
                EF.Functions.Like(x.DatabaseName, search) ||
                EF.Functions.Like(x.Username, search));
        }

        query = query.OrderByDynamic(
            request.Page.SortBy ?? nameof(ConnectionConfig.UpdatedAt),
            desc: request.Page.Desc);

        var projected = query.Select(x => x.MapToResponse());

        var page = await projected.ToPageResultAsync(
            request.Page.PageNumber,
            request.Page.Size,
            cancellationToken);

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
        var response = await mediator.Send(new GetConnectionConfigListQuery(search,
            new PageRequest(page, pageSize, sortBy, desc)), cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}