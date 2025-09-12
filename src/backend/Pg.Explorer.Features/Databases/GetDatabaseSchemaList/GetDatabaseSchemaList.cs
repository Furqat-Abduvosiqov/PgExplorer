using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Features.Databases.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;
using Pg.Explorer.Shared.Pagination;

namespace Pg.Explorer.Features.Databases.GetDatabaseSchemaList;

public record GetDatabaseSchemaListQuery(
    string? Search,
    PageRequest Page,
    long ConnectionId,
    string DatabaseName
) : IRequest<ErrorOr<PageResult<SchemaInfo>>>;

internal sealed class GetDatabaseSchemaListQueryHandler(IDatabaseService databaseService)
    : IRequestHandler<GetDatabaseSchemaListQuery, ErrorOr<PageResult<SchemaInfo>>>
{
    public async Task<ErrorOr<PageResult<SchemaInfo>>> Handle(
        GetDatabaseSchemaListQuery query,
        CancellationToken cancellationToken)
    {
        var schemasResult = await databaseService.GetSchemasAsync(
            query.ConnectionId,
            query.DatabaseName,
            cancellationToken);

        if (schemasResult.IsError)
            return schemasResult.FirstError;

        var schemas = schemasResult.Value.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant();
            schemas = schemas.Where(s =>
                s.Name.ToLower().Contains(search) ||
                s.Owner.ToLower().Contains(search));
        }

        schemas = schemas.OrderByDynamic(
            query.Page.SortBy ?? nameof(SchemaInfo.Name),
            desc: query.Page.Desc);

        var page = await schemas.ToPageResultAsync(
            query.Page.PageNumber,
            query.Page.Size,
            cancellationToken);

        return page;
    }
}

public class GetDatabaseSchemaListEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/databases/{databaseName}/schemas", Handle)
            .WithTags("Databases");
    }

    private static async Task<IResult> Handle(
        IMediator mediator,
        [FromRoute] string databaseName,
        [FromQuery] string? search,
        [FromQuery] long connectionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        CancellationToken cancellationToken = default)
    {
        if (connectionId <= 0)
            return Results.BadRequest("ConnectionId is a required parameter");

        if (string.IsNullOrWhiteSpace(databaseName))
            return Results.BadRequest("DatabaseName is a required parameter");

        var response = await mediator.Send(
            new GetDatabaseSchemaListQuery(
                search,
                new PageRequest(page, pageSize, sortBy, desc),
                connectionId,
                databaseName),
            cancellationToken);

        return response.IsError
            ? response.Errors.ToProblem()
            : Results.Ok(response.Value);
    }
}