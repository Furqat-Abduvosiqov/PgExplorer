using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Features.Databases.Shared.Services;
using Pg.Explorer.Features.Tables.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Databases.GetTableInfoBySchemaName;

internal sealed record GetTableInfoQuery(
    long ConnectionId,
    string SchemaName,
    string TableName) : IRequest<ErrorOr<TableInfo>>;

internal sealed class GetTableInfoQueryHandler(
    IDatabaseService databaseService)
    : IRequestHandler<GetTableInfoQuery, ErrorOr<TableInfo>>
{
    public async Task<ErrorOr<TableInfo>> Handle(
        GetTableInfoQuery request,
        CancellationToken cancellationToken)
    {
        return await databaseService.GetTableInfoAsync(
            request.ConnectionId,
            request.SchemaName,
            request.TableName,
            cancellationToken);
    }
}

public class GetTableInfoEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/tables/{tableName}", Handle)
            .WithTags("Tables");
    }

    private static async Task<IResult> Handle(
        [FromQuery] long connectionId,
        [FromQuery] string schemaName,
        [FromRoute] string tableName,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (connectionId <= 0)
            return Results.BadRequest("ConnectionId required parameter");

        if (string.IsNullOrEmpty(schemaName))
            return Results.BadRequest("SchemaName required parameter");

        if (string.IsNullOrEmpty(tableName))
            return Results.BadRequest("TableName required parameter");

        var query = new GetTableInfoQuery(connectionId, schemaName, tableName);
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsError)
            return result.Errors.ToProblem();

        return Results.Ok(result.Value);
    }
}