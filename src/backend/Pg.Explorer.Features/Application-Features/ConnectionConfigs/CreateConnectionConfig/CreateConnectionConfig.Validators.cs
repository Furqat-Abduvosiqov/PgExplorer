using FluentValidation;

namespace Pg.Explorer.Features.Application_Features.ConnectionConfigs.CreateConnectionConfig;

public class CreateConnectionConfigValidators : AbstractValidator<CreateConnectionConfigRequest>
{
    public CreateConnectionConfigValidators()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        RuleFor(x => x.Host).NotEmpty().WithMessage("Host is required.");
        RuleFor(x => x.Port).NotEmpty().NotEqual(0).WithMessage("Port is required.");
        RuleFor(x => x.DatabaseName).NotEmpty().WithMessage("Database name is required.");
    }
}