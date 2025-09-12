using System.Text;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Features.Tables.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Tables.ExportTable;

public sealed record ExportTableRequest(
    long ConnectionId,
    string SchemaName,
    string TableName
);

public sealed record ExportTableCommand(
    long ConnectionId,
    string SchemaName,
    string TableName
) : IRequest<ErrorOr<string>>;

internal sealed class ExportTableCommandHandler(
    ITableService tableService
) : IRequestHandler<ExportTableCommand, ErrorOr<string>>
{
    public Task<ErrorOr<string>> Handle(ExportTableCommand command, CancellationToken cancellationToken) =>
        tableService.ExportTableToCsvAsync(command, cancellationToken);
}

public class ExportTableEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/tables/export", Handle)
            .WithTags("Tables");
    }

    private static async Task<IResult> Handle(
        [FromBody] ExportTableRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<ExportTableRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var command = request.MapToCommand();

        var response = await mediator.Send(command, cancellationToken);

        if (response.IsError)
            return response.Errors.ToProblem();

        var csvBytes = Encoding.UTF8.GetBytes(response.Value);

        return Results.File(
            csvBytes,
            contentType: "text/csv",
            fileDownloadName: $"{request.TableName}.csv"
        );
    }
}