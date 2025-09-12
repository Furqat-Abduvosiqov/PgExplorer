using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Features.Tables.Shared;
using Pg.Explorer.Features.Tables.Shared.Services;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Tables.GetTableData;

public sealed record GetTableDataQuery(
    long ConnectionId,
    string SchemaName,
    string TableName,
    int Page = 1,
    int PageSize = 50
);

public sealed record GetTableDataCommand(
    long ConnectionId,
    string SchemaName,
    string TableName,
    int Page,
    int PageSize
) : IRequest<ErrorOr<ExecutionResult>>;

internal sealed class GetTableDataCommandHandler(
    ITableService tableService,
    ILogger<GetTableDataCommandHandler> logger
) : IRequestHandler<GetTableDataCommand, ErrorOr<ExecutionResult>>
{
    public async Task<ErrorOr<ExecutionResult>> Handle(GetTableDataCommand command, CancellationToken cancellationToken)
    {
        var result = await tableService.GetTableDataAsync(command, cancellationToken);
        logger.LogInformation("Logged table read query for {Schema}.{Table}", command.SchemaName, command.TableName);

        return result;
    }
}

public class GetTableDataEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/tables/{tableName}/data", Handle)
            .WithTags("Tables");
    }

    private static async Task<IResult> Handle(
        [FromServices] IMediator mediator,
        [FromServices] IValidator<GetTableDataQuery> validator,
        [FromRoute] string tableName,
        [FromQuery] long connectionId,
        [FromQuery] string schemaName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var request = new GetTableDataQuery(connectionId, schemaName, tableName, page, pageSize);

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var command = request.MapToCommand();

        var response = await mediator.Send(command, cancellationToken);
        return response.IsError ? response.Errors.ToProblem() : Results.Ok(response.Value);
    }
}