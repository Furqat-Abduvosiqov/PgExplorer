using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Domain.Connections;
using Pg.Explorer.Domain.Queries;
using Pg.Explorer.Domain.Queries.Entities;
using Pg.Explorer.Features.Queries.Shared;
using Pg.Explorer.Features.Queries.Shared.Services;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Queries.CreateQuery;

public sealed record CreateQueryRequest(
    long ConnectionId,
    string QueryBody,
    QueryType QueryType);

public sealed record CreateQueryCommand(
    long ConnectionId,
    string QueryBody,
    QueryType QueryType)
    : IRequest<ErrorOr<QueryResponse>>;

internal sealed class CreateQueryCommandHandler(
    IQueryEntityRepository queryRepository,
    IQueryService queryService,
    IConnectionRepository connectionRepository,
    ILogger<CreateQueryCommandHandler> logger)
    : IRequestHandler<CreateQueryCommand, ErrorOr<QueryResponse>>
{
    public async Task<ErrorOr<QueryResponse>> Handle(CreateQueryCommand command, CancellationToken cancellationToken)
    {
        var connection = await connectionRepository.GetByIdAsync(command.ConnectionId, cancellationToken);
        if (connection is null)
        {
            return Error.NotFound("Connection.NotFound", $"Connection with id {command.ConnectionId} not found.");
        }

        var entity = command.MapToQuery();

        await queryService.ExecuteQueryAsync(entity, connection, cancellationToken);

        await queryRepository.AddAsync(entity, cancellationToken);
        await queryRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created query: {@Query}", entity);

        var response = entity.MapToResponse();
        return response;
    }
}

public class CreateQueryEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/queries", Handle)
            .WithTags("Queries");
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateQueryRequest request,
        IValidator<CreateQueryRequest> validator,
        IMediator mediator,
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
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}