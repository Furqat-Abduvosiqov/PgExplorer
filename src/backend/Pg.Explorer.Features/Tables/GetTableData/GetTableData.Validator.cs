using FluentValidation;

namespace Pg.Explorer.Features.Tables.GetTableData;

public class GetTableDataValidator : AbstractValidator<GetTableDataRequest>
{
    public GetTableDataValidator()
    {
        RuleFor(x => x.ConnectionId)
            .NotNull().WithMessage("ConnectionId is required.")
            .GreaterThan(0).WithMessage("ConnectionId must be greater than zero");

        RuleFor(x => x.SchemaName)
            .NotNull().WithMessage("SchemaName is required.")
            .NotEmpty().WithMessage("SchemaName must be supplied.");

        RuleFor(x => x.TableName)
            .NotNull().WithMessage("TableName is required.")
            .NotEmpty().WithMessage("TableName must be supplied.");
    }
}