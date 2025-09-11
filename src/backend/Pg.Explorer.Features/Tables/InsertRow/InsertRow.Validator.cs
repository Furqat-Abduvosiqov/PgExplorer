using FluentValidation;

namespace Pg.Explorer.Features.Tables.InsertRow;

public class InsertRowValidator : AbstractValidator<InsertRowCommand>
{
    public InsertRowValidator()
    {
        RuleFor(x => x.TableName).NotEmpty().NotNull().WithMessage("TableName is required");
        RuleFor(x => x.SchemaName).NotEmpty().NotNull().WithMessage("SchemaName is required");
        RuleFor(x => x.ConnectionId).GreaterThan(0).WithMessage("Connection id is required");
        RuleFor(x => x.RowData).NotNull().WithMessage("RowData is required");
    }
}