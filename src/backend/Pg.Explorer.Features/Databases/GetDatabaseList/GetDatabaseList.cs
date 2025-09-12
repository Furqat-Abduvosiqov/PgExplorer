using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Features.Databases.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;
using Pg.Explorer.Shared.Pagination;

namespace Pg.Explorer.Features.Databases.GetDatabaseList;

public record GetDatabaseListQuery(string? Search, PageRequest Page, long ConnectionId)
    : IRequest<ErrorOr<PageResult<DatabaseInfo>>>;

internal sealed class GetDatabaseListQueryHandler(IDatabaseService databaseService)
    : IRequestHandler<GetDatabaseListQuery, ErrorOr<PageResult<DatabaseInfo>>>
{
    public async Task<ErrorOr<PageResult<DatabaseInfo>>> Handle(
        GetDatabaseListQuery query,
        CancellationToken cancellationToken)
    {
        var databasesResult = await databaseService.GetDatabasesAsync(query.ConnectionId, cancellationToken);

        if (databasesResult.IsError)
            return databasesResult.FirstError;

        var databases = databasesResult.Value.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant();
            databases = databases.Where(db =>
                db.Name.ToLower().Contains(search) ||
                db.Owner.ToLower().Contains(search));
        }

        databases = databases.OrderByDynamic(
            query.Page.SortBy ?? nameof(DatabaseInfo.Name),
            desc: query.Page.Desc);

        var page = await databases.ToPageResultAsync(
            query.Page.PageNumber,
            query.Page.Size,
            cancellationToken);

        return page;
    }
}

public class GetDatabaseListEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/databases", Handle)
            .WithTags("Databases");
    }

    private static async Task<IResult> Handle(
        IMediator mediator,
        [FromQuery] string? search,
        [FromQuery] long connectionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        CancellationToken cancellationToken = default)
    {
        if (connectionId <= 0)
            return Results.BadRequest("ConnectionId required parameter");

        var response = await mediator.Send(
            new GetDatabaseListQuery(search,
                new PageRequest(page, pageSize, sortBy, desc),
                connectionId),
            cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}