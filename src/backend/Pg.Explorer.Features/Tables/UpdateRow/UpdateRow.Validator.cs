using FluentValidation;

namespace Pg.Explorer.Features.Tables.UpdateRow;

public class UpdateRowValidator : AbstractValidator<UpdateRowRequest>
{
    public UpdateRowValidator()
    {
        RuleFor(x => x.RowData).NotNull().WithMessage("RowData is required");
        RuleFor(x => x.WhereConditions).NotNull().WithMessage("Conditions are required");
        RuleFor(x => x.ConnectionId).NotNull().GreaterThan(0).WithMessage("ConnectionId is required");
        RuleFor(x => x.TableName).NotNull().NotEmpty().WithMessage("TableName is required");
        RuleFor(x => x.SchemaName).NotNull().NotEmpty().WithMessage("SchemaName is required");
    }
}