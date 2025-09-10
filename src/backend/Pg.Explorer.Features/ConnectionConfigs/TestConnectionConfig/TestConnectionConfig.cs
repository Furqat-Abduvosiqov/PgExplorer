using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pg.Explorer.Domain.ConnectionConfigurations;
using Pg.Explorer.Features.Abstractions;
using Pg.Explorer.Features.ConnectionConfigs.Shared.Services;
using Pg.Explorer.Features.Extensions;

namespace Pg.Explorer.Features.ConnectionConfigs.TestConnectionConfig;

public sealed record TestConnectionConfigCommand(long ConnectionId) : IRequest<ErrorOr<bool>>;

internal sealed class TestConnectionConfigCommandHandler(
    IConnectionConfigService connectionConfigService,
    IConnectionConfigRepository repository,
    ILogger<TestConnectionConfigCommandHandler> logger)
    : IRequestHandler<TestConnectionConfigCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(
        TestConnectionConfigCommand request,
        CancellationToken cancellationToken)
    {
        var connection = await repository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection is null)
        {
            logger.LogError($"Connection {request.ConnectionId} not found.");

            return Error.NotFound(code: "ConnectionConfig.NotFound",
                description: $"Connection config {request.ConnectionId} not found.");
        }

        var testResult = await connectionConfigService.TestConnectionAsync(connection);

        return testResult;
    }
}

public class TestConnectionConfigEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/connection-configs/{id}/test", Handle)
            .WithTags("ConnectionConfigs");
    }

    private static async Task<IResult> Handle(
        [FromRoute] long id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["id"] = ["Connection ID is required and must be greater than zero."]
            });


        var command = new TestConnectionConfigCommand(id);

        var response = await mediator.Send(command, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}