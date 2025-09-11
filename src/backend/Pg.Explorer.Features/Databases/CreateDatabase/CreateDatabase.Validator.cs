using FluentValidation;

namespace Pg.Explorer.Features.Databases.CreateDatabase;

public class CreateDatabaseRequestValidator : AbstractValidator<CreateDatabaseRequest>
{
    public CreateDatabaseRequestValidator()
    {
        RuleFor(x => x.ConnectionId)
            .GreaterThan(0).WithMessage("ConnectionId must be greater than 0");

        RuleFor(x => x.DatabaseName)
            .NotEmpty().WithMessage("Database name is required")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Database name can only contain letters, numbers, and underscores");
    }
}