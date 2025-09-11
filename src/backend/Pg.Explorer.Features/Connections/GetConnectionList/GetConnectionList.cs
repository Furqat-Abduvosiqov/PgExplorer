using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;
using Pg.Explorer.Shared.Pagination;

namespace Pg.Explorer.Features.Connections.GetConnectionList;

public sealed record GetConnectionListQuery(string? Search, PageRequest Page)
    : IRequest<ErrorOr<PageResult<ConnectionResponse>>>;

internal sealed class GetConnectionListQueryHandler(IConnectionRepository repository)
    : IRequestHandler<GetConnectionListQuery, ErrorOr<PageResult<ConnectionResponse>>>
{
    public async Task<ErrorOr<PageResult<ConnectionResponse>>> Handle(
        GetConnectionListQuery request,
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
            request.Page.SortBy ?? nameof(Connection.UpdatedAt),
            desc: request.Page.Desc);

        var projected = query.Select(x => x.MapToResponse());

        var page = await projected.ToPageResultAsync(
            request.Page.PageNumber,
            request.Page.Size,
            cancellationToken);

        return page;
    }
}

public class GetConnectionListEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/connections", Handle)
            .WithTags("Connections");
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
        var response = await mediator.Send(new GetConnectionListQuery(search,
            new PageRequest(page, pageSize, sortBy, desc)), cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}