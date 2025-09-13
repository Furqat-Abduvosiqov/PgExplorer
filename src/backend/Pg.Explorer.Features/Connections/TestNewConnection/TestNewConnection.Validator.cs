using FluentValidation;

namespace Pg.Explorer.Features.Connections.TestNewConnection;

public class TestConnectionRequestValidator : AbstractValidator<TestNewConnectionRequest>
{
    public TestConnectionRequestValidator()
    {
        RuleFor(x => x.DatabaseName).NotNull().NotEmpty().WithMessage("Database name is required.");
        RuleFor(x => x.Host).NotNull().NotEmpty().WithMessage("Host name is required.");
        RuleFor(x => x.Port).NotNull().GreaterThan(0).WithMessage("Port is required.");
        RuleFor(x => x.Username).NotNull().NotEmpty().WithMessage("Username is required.");
        RuleFor(x => x.Password).NotNull().NotEmpty().WithMessage("Password is required.");
    }
}