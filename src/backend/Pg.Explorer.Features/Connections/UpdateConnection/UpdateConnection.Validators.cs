using FluentValidation;

namespace Pg.Explorer.Features.Connections.UpdateConnection;

public class UpdateConnectionValidators : AbstractValidator<UpdateConnectionRequest>
{
    public UpdateConnectionValidators()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Host).NotEmpty();
        RuleFor(x => x.Port).NotEmpty().NotEqual(0);
        RuleFor(x => x.DatabaseName).NotEmpty();
    }
}