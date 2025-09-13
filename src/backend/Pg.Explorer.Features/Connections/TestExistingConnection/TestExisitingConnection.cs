using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Connections.TestExistingConnection;

public sealed record TestExistingConnectionCommand(long ConnectionId) : IRequest<ErrorOr<bool>>;

internal sealed class TestExistingConnectionCommandHandler(
    IConnectionService connectionService,
    IConnectionRepository repository,
    ILogger<TestExistingConnectionCommandHandler> logger)
    : IRequestHandler<TestExistingConnectionCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(
        TestExistingConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var connection = await repository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
        {
            logger.LogError($"Connection {request.ConnectionId} not found.");

            return Error.NotFound(code: "Connection.NotFound",
                description: $"Connection with id {request.ConnectionId} not found.");
        }

        var testResult = await connectionService.TestExistingConnectionAsync(connection);

        return testResult;
    }
}

public class TestExistingConnectionEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/connections/{id}/test", Handle)
            .WithTags("Connections");
    }

    private static async Task<IResult> Handle(
        [FromRoute] long id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Results.BadRequest("Invalid id.");

        var command = new TestExistingConnectionCommand(id);

        var response = await mediator.Send(command, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}