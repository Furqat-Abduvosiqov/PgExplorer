using FluentValidation;

namespace Pg.Explorer.Features.Queries.CreateQuery;

public class CreateQueryValidator : AbstractValidator<CreateQueryRequest>
{
    public CreateQueryValidator()
    {
        RuleFor(x => x.ConnectionId).NotEmpty().GreaterThan(0).WithMessage("ConnectionId must be greater than zero");
        RuleFor(x => x.QueryBody).NotEmpty().WithMessage("QueryBody must be set");
        RuleFor(x => x.QueryType).IsInEnum().WithMessage("QueryType must be set");
    }
}