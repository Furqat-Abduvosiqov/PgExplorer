using FluentValidation;

namespace Pg.Explorer.Features.Queries.UpdateQuery;

public class UpdateQueryValidators : AbstractValidator<UpdateQueryRequest>
{
    public UpdateQueryValidators()
    {
        RuleFor(x => x.QueryBody).NotEmpty().NotNull().WithMessage("QueryBody is required.");
        RuleFor(x => x.QueryType).NotNull().IsInEnum().WithMessage("QueryType is required.");
    }
}