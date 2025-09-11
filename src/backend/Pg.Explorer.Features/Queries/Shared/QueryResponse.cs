using Pg.Explorer.Domain.Queries.Entities;

namespace Pg.Explorer.Features.Queries.Shared;

public sealed record QueryResponse(
    Guid Id,
    long ConnectionId,
    string QueryBody,
    QueryType QueryType,
    DateTimeOffset ExecutedAt,
    TimeSpan ExecutionTime,
    QueryStatus QueryStatus,
    string? ErrorMessage,
    int AffectedRows);