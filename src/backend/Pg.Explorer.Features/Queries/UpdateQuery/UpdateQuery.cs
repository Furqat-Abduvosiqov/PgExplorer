using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Features.Abstractions;
using Pg.Explorer.Features.Extensions;
using Pg.Explorer.Features.Queries.Shared;
using Pg.Explorer.Features.Queries.Shared.Services;

namespace Pg.Explorer.Features.Queries.UpdateQuery;

public sealed record UpdateQueryRequest(
    long ConnectionId,
    string QueryBody,
    QueryType QueryType);

public sealed record UpdateQueryCommand(
    Guid Id,
    long ConnectionId,
    string QueryBody,
    QueryType QueryType)
    : IRequest<ErrorOr<QueryResponse>>;

internal sealed class UpdateQueryCommandHandler(
    IQueryEntityRepository repository,
    IConnectionConfigRepository connectionRepository,
    IQueryService queryService,
    ILogger<UpdateQueryCommandHandler> logger)
    : IRequestHandler<UpdateQueryCommand, ErrorOr<QueryResponse>>
{
    public async Task<ErrorOr<QueryResponse>> Handle(UpdateQueryCommand request, CancellationToken cancellationToken)
    {
        var connection = await connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
        {
            return Error.NotFound("Connection.NotFound", $"Connection {request.ConnectionId} not found.");
        }

        var query = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (query is null)
        {
            return Error.NotFound("Query.NotFound", $"Query {request.Id} not found.");
        }

        query.QueryBody = request.QueryBody;
        query.QueryType = request.QueryType;

        await queryService.ExecuteQueryAsync(query, connection, cancellationToken);

        repository.Update(query);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated query: {@Query}", query);
        return query.MapToResponse();
    }
}

public class UpdateQueryEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("/api/queries/{id:guid}", Handle)
            .WithTags("Queries");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateQueryRequest request,
        IValidator<UpdateQueryRequest> validator,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        if (id == Guid.Empty)
        {
            return Results.BadRequest("Id is required.");
        }

        var command = request.MapToCommand(id);

        var response = await mediator.Send(command, cancellationToken);

        return response.IsError ? response.Errors.ToProblem() : Results.Ok(response.Value);
    }
}