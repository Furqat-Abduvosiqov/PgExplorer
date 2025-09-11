using FluentValidation;

namespace Pg.Explorer.Features.Tables.ExportTable;

public class ExportTableValidator : AbstractValidator<ExportTableRequest>
{
    public ExportTableValidator()
    {
        RuleFor(x => x.TableName).NotNull().NotEmpty().WithMessage("TableName is required.");
        RuleFor(x => x.SchemaName).NotEmpty().WithMessage("SchemaName is required.");
        RuleFor(x => x.ConnectionId).GreaterThan(0).WithMessage("ConnectionId must be greater than zero.");
    }
}