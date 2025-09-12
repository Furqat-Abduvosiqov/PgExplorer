using ErrorOr;
using Pg.Explorer.Features.Tables.Shared;

namespace Pg.Explorer.Features.Databases.Shared;

public interface IDatabaseService
{
    Task<ErrorOr<IEnumerable<DatabaseInfo>>> GetDatabasesAsync(long connectionId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<IEnumerable<SchemaInfo>>> GetSchemasAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<IEnumerable<TableInfo>>> GetTablesAsync(long connectionId, string schemaName,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<TableInfo>> GetTableInfoAsync(long connectionId, string schemaName, string tableName,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<bool>> CreateDatabaseAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<bool>> DropDatabaseAsync(long connectionId, string databaseName,
        CancellationToken cancellationToken = default);
}