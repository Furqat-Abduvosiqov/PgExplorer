using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Connections.TestConnection;

public sealed record TestConnectionCommand(long ConnectionId) : IRequest<ErrorOr<bool>>;

internal sealed class TestConnectionCommandHandler(
    IConnectionService connectionService,
    IConnectionRepository repository,
    ILogger<TestConnectionCommandHandler> logger)
    : IRequestHandler<TestConnectionCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(
        TestConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var connection = await repository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
        {
            logger.LogError($"Connection {request.ConnectionId} not found.");

            return Error.NotFound(code: "Connection.NotFound",
                description: $"Connection with id {request.ConnectionId} not found.");
        }

        var testResult = await connectionService.TestConnectionAsync(connection);

        return testResult;
    }
}

public class TestConnectionEndpoint : IEndpoint
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

        var command = new TestConnectionCommand(id);

        var response = await mediator.Send(command, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}