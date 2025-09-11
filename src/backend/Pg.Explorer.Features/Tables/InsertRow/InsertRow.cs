using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Features.Tables.Shared;
using Pg.Explorer.Features.Tables.Shared.Services;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Tables.InsertRow;

public sealed record InsertRowRequest(
    long ConnectionId,
    string SchemaName,
    string TableName,
    IReadOnlyDictionary<string, object?> RowData
);

public sealed record InsertRowCommand(
    long ConnectionId,
    string SchemaName,
    string TableName,
    IReadOnlyDictionary<string, object?> RowData
) : IRequest<ErrorOr<ExecutionResult>>;

internal sealed class InsertRowCommandHandler(
    ITableService tableService
) : IRequestHandler<InsertRowCommand, ErrorOr<ExecutionResult>>
{
    public async Task<ErrorOr<ExecutionResult>> Handle(InsertRowCommand command,
        CancellationToken cancellationToken) => await tableService.InsertRowAsync(command, cancellationToken);
}

public class InsertRowEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/tables/rows", Handle)
            .WithTags("Tables");
    }

    private static async Task<IResult> Handle(
        [FromBody] InsertRowRequest request,
        [FromServices] IValidator<InsertRowRequest> validator,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var command = request.MappToCommand();

        var response = await mediator.Send(command, cancellationToken);
        return response.IsError ? response.Errors.ToProblem() : Results.Ok(response.Value);
    }
}