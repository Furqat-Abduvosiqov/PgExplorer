using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Pg.Explorer.Shared.Endpoints.Extensions;

public static class EndpointResultsExtensions
{
    public static IResult ToProblem(this Error error)
    {
        return CreateProblem(error);
    }

    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Results.Problem();
        }

        return CreateProblem(errors);
    }

    private static IResult CreateProblem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Failure => StatusCodes.Status417ExpectationFailed,
            _ => StatusCodes.Status500InternalServerError
        };

        var dictionary = new Dictionary<string, string[]>
        {
            [error.Code] = [error.Description]
        };

        return Results.ValidationProblem(dictionary, statusCode: statusCode);
    }

    private static IResult CreateProblem(List<Error> errors)
    {
        var statusCode = errors.First().Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Failure => StatusCodes.Status417ExpectationFailed,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.ValidationProblem(errors.ToDictionary(k => k.Code, v => new[] { v.Description }),
            statusCode: statusCode);
    }
}