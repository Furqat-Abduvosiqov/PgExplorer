using FluentValidation;

namespace Pg.Explorer.Features.Connections.UpdateConnection;

public class UpdateConnectionValidator : AbstractValidator<UpdateConnectionRequest>
{
    public UpdateConnectionValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Host).NotEmpty();
        RuleFor(x => x.Port).NotEmpty().NotEqual(0);
        RuleFor(x => x.DatabaseName).NotEmpty();
    }
}