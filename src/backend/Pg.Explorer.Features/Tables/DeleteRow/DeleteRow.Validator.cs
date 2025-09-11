using FluentValidation;

namespace Pg.Explorer.Features.Tables.DeleteRow;

public class DeleteRowValidator : AbstractValidator<DeleteRowRequest>
{
    public DeleteRowValidator()
    {
        RuleFor(x => x.ConnectionId).GreaterThan(0).WithMessage("ConnectionId must be greater than zero");
        RuleFor(x => x.SchemaName).NotNull().NotEmpty().WithMessage("SchemaName is required");
        RuleFor(x => x.TableName).NotNull().NotEmpty().WithMessage("TableName is required");
        RuleFor(x => x.WhereConditions).NotNull().WithMessage("WhereConditions are required");
    }
}