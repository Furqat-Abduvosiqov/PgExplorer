using ErrorOr;
using Pg.Explorer.Features.Tables.ExportTable;
using Pg.Explorer.Features.Tables.GetTableData;
using Pg.Explorer.Features.Tables.InsertRow;

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
        UpdateRowRequest request,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<ExecutionResult>> DeleteRowAsync(
        DeleteRowRequest request,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<string>> ExportTableToCsvAsync(
        ExportTableRequest request,
        CancellationToken cancellationToken = default);
}