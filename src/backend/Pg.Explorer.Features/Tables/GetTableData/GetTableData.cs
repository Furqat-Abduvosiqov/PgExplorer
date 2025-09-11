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

public sealed record GetTableDataRequest(
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
        app.MapPost("/api/tables/data", Handle)
            .WithTags("Tables");
    }

    private static async Task<IResult> Handle(
        [FromBody] GetTableDataRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<GetTableDataRequest> validator,
        CancellationToken cancellationToken)
    {
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