using ErrorOr;
using Pg.Explorer.Features.Tables.DeleteRow;
using Pg.Explorer.Features.Tables.ExportTable;
using Pg.Explorer.Features.Tables.GetTableData;
using Pg.Explorer.Features.Tables.InsertRow;
using Pg.Explorer.Features.Tables.UpdateRow;

namespace Pg.Explorer.Features.Tables.Shared.Services;

public interface ITableService
{
    Task<ErrorOr<ExecutionResult>> GetTableDataAsync(
        GetTableDataCommand request,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<ExecutionResult>> InsertRowAsync(
        InsertRowCommand request,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<ExecutionResult>> UpdateRowAsync(
        UpdateRowCommand request,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<ExecutionResult>> DeleteRowAsync(
        DeleteRowCommand request,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<string>> ExportTableToCsvAsync(
        ExportTableCommand request,
        CancellationToken cancellationToken = default);
}