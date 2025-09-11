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

namespace Pg.Explorer.Features.Tables.DeleteRow;

public sealed record DeleteRowRequest(
    long ConnectionId,
    string SchemaName,
    string TableName,
    IReadOnlyDictionary<string, object?> WhereConditions
);

public sealed record DeleteRowCommand(
    long ConnectionId,
    string SchemaName,
    string TableName,
    IReadOnlyDictionary<string, object?> WhereConditions
) : IRequest<ErrorOr<ExecutionResult>>;

internal sealed class DeleteRowCommandHandler(
    ITableService tableService
) : IRequestHandler<DeleteRowCommand, ErrorOr<ExecutionResult>>
{
    public async Task<ErrorOr<ExecutionResult>> Handle(DeleteRowCommand command,
        CancellationToken cancellationToken) =>
        await tableService.DeleteRowAsync(command, cancellationToken);
}

public class DeleteRowEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/tables/rows", Handle)
            .WithTags("Tables");
    }

    private static async Task<IResult> Handle(
        [FromBody] DeleteRowRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<DeleteRowRequest> validator,
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