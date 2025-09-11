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

namespace Pg.Explorer.Features.Tables.UpdateRow;

public sealed record UpdateRowRequest(
    long ConnectionId,
    string SchemaName,
    string TableName,
    IReadOnlyDictionary<string, object?> RowData,
    IReadOnlyDictionary<string, object?> WhereConditions
);

public sealed record UpdateRowCommand(
    long ConnectionId,
    string SchemaName,
    string TableName,
    IReadOnlyDictionary<string, object?> RowData,
    IReadOnlyDictionary<string, object?> WhereConditions
) : IRequest<ErrorOr<ExecutionResult>>;

internal sealed class UpdateRowCommandHandler(
    ITableService tableService
) : IRequestHandler<UpdateRowCommand, ErrorOr<ExecutionResult>>
{
    public async Task<ErrorOr<ExecutionResult>> Handle(UpdateRowCommand command,
        CancellationToken cancellationToken) => await tableService.UpdateRowAsync(command, cancellationToken);
}

public class UpdateRowEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("/api/tables/rows", Handle)
            .WithTags("Tables");
    }

    private static async Task<IResult> Handle(
        [FromBody] UpdateRowRequest request,
        [FromServices] IValidator<UpdateRowRequest> validator,
        [FromServices] IMediator mediator,
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