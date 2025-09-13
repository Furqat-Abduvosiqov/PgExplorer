using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pg.Explorer.Features.Connections.Shared;
using Pg.Explorer.Shared.Endpoints.Abstractions;
using Pg.Explorer.Shared.Endpoints.Extensions;

namespace Pg.Explorer.Features.Connections.TestNewConnection;

public sealed record TestNewConnectionRequest(
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string Password);

public sealed record TestNewConnectionCommand(
    string Host,
    int Port,
    string DatabaseName,
    string Username,
    string Password) : IRequest<ErrorOr<bool>>;

internal sealed class TestNewConnectionCommandHandler(IConnectionService connectionService)
    : IRequestHandler<TestNewConnectionCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(
        TestNewConnectionCommand command,
        CancellationToken cancellationToken) => await connectionService.TestNewConnectionAsync(command);
}

public class TestNewConnectionEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/connections/test", Handle)
            .WithTags("Connections");
    }

    private static async Task<IResult> Handle(
        [FromBody] TestNewConnectionRequest request,
        IMediator mediator,
        IValidator<TestNewConnectionRequest> validator,
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