namespace Pg.Explorer.Shared.Pagination;

public sealed record PageRequest(int Page = 1, int PageSize = 20, string? SortBy = null, bool Desc = false)
{
    public int PageNumber => Page < 1 ? 1 : Page;
    public int Size => PageSize is < 1 or > 500 ? 20 : PageSize;
}