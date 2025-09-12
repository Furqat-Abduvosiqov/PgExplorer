using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Features.Queries.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;
using Pg.Explorer.Shared.Pagination;

namespace Pg.Explorer.Features.Connections.GetConnectionQueries;

public sealed record GetConnectionQueries(long ConnectionId, string? Search, PageRequest Page)
    : IRequest<ErrorOr<PageResult<QueryResponse>>>;

internal sealed class GetConnectionQueriesHandler(IQueryEntityRepository repository)
    : IRequestHandler<GetConnectionQueries, ErrorOr<PageResult<QueryResponse>>>
{
    public async Task<ErrorOr<PageResult<QueryResponse>>> Handle(
        GetConnectionQueries request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query()
            .Where(q => q.ConnectionId == request.ConnectionId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search}%";
            query = query.Where(q =>
                EF.Functions.Like(q.QueryBody, search) ||
                EF.Functions.Like(q.ErrorMessage ?? string.Empty, search));
        }

        query = query.OrderByDynamic(
            request.Page.SortBy ?? nameof(QueryEntity.ExecutedAt),
            desc: request.Page.Desc);

        var projected = query.Select(q => q.MapToResponse());

        var page = await projected.ToPageResultAsync(
            request.Page.PageNumber,
            request.Page.Size,
            cancellationToken);

        return page;
    }
}

public class GetConnectionQueriesEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/connections/{id}/queries", Handle)
            .WithTags("Connections");
    }

    private static async Task<IResult> Handle(
        IMediator mediator,
        [FromRoute] long id,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new GetConnectionQueries(id, search,
            new PageRequest(page, pageSize, sortBy, desc)), cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}