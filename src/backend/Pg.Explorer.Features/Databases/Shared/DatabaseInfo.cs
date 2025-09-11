namespace Pg.Explorer.Features.Databases.Shared;

public sealed record DatabaseInfo(string Name, string Owner, long Size, string Encoding, int TableCount);