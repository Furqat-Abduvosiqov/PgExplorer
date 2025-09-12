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

namespace Pg.Explorer.Features.Queries.GetQueryList;

public sealed record GetQueryListQuery(string? Search, PageRequest Page) : IRequest<ErrorOr<PageResult<QueryResponse>>>;

internal sealed class GetQueryListQueryHandler(IQueryEntityRepository repository)
    : IRequestHandler<GetQueryListQuery, ErrorOr<PageResult<QueryResponse>>>
{
    public async Task<ErrorOr<PageResult<QueryResponse>>> Handle(GetQueryListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query();


        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(x => EF.Functions.Like(x.QueryBody, $"%{request.Search}%"));
        }


        query = query.OrderByDynamic(
            request.Page.SortBy ?? nameof(QueryEntity.ExecutedAt),
            desc: request.Page.Desc);


        var projected = query.Select(x => x.MapToResponse());


        var page = await projected.ToPageResultAsync(
            request.Page.PageNumber,
            request.Page.Size,
            cancellationToken);

        return page;
    }
}

public class GetQueryListEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/queries", Handle)
            .WithTags("Queries");
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
        var response = await mediator.Send(new GetQueryListQuery(search, new PageRequest(page, pageSize, sortBy, desc)),
            cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}