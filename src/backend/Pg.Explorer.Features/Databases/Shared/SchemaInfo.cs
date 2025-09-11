namespace Pg.Explorer.Features.Databases.Shared;

public record SchemaInfo(string Name, string Owner, int TableCount);