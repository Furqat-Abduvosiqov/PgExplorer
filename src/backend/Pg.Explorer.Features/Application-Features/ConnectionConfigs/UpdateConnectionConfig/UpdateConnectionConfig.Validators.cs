using FluentValidation;

namespace Pg.Explorer.Features.Application_Features.ConnectionConfigs.UpdateConnectionConfig;

public class UpdateConnectionConfigValidators : AbstractValidator<UpdateConnectionConfigRequest>
{
    public UpdateConnectionConfigValidators()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Host).NotEmpty();
        RuleFor(x => x.Port).NotEmpty().NotEqual(0);
        RuleFor(x => x.DatabaseName).NotEmpty();
    }
}