using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Features.Databases.Shared.Services;
using Pg.Explorer.Features.Tables.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;
using Pg.Explorer.Shared.Pagination;

namespace Pg.Explorer.Features.Databases.GetTableListBySchemaName;

public record GetTableListQuery(
    string? Search,
    PageRequest Page,
    long ConnectionId,
    string SchemaName
) : IRequest<ErrorOr<PageResult<TableInfo>>>;

internal sealed class GetTableListQueryHandler(IDatabaseService databaseService)
    : IRequestHandler<GetTableListQuery, ErrorOr<PageResult<TableInfo>>>
{
    public async Task<ErrorOr<PageResult<TableInfo>>> Handle(
        GetTableListQuery query,
        CancellationToken cancellationToken)
    {
        var tablesResult = await databaseService.GetTablesAsync(
            query.ConnectionId,
            query.SchemaName,
            cancellationToken);

        if (tablesResult.IsError)
            return tablesResult.FirstError;

        var tables = tablesResult.Value.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant();
            tables = tables.Where(t =>
                t.TableName.ToLower().Contains(search) ||
                t.TableType.ToLower().Contains(search));
        }

        tables = tables.OrderByDynamic(
            query.Page.SortBy ?? nameof(TableInfo.TableName),
            desc: query.Page.Desc);

        var page = await tables.ToPageResultAsync(
            query.Page.PageNumber,
            query.Page.Size,
            cancellationToken);

        return page;
    }
}

public class GetTableListEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/tables", Handle)
            .WithTags("Tables");
    }

    private static async Task<IResult> Handle(
        IMediator mediator,
        [FromQuery] string schemaName,
        [FromQuery] long connectionId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        CancellationToken cancellationToken = default)
    {
        if (connectionId <= 0)
            return Results.BadRequest("ConnectionId is a required parameter");

        if (string.IsNullOrWhiteSpace(schemaName))
            return Results.BadRequest("SchemaName is a required parameter");

        var response = await mediator.Send(
            new GetTableListQuery(
                search,
                new PageRequest(page, pageSize, sortBy, desc),
                connectionId,
                schemaName),
            cancellationToken);

        return response.IsError
            ? response.Errors.ToProblem()
            : Results.Ok(response.Value);
    }
}