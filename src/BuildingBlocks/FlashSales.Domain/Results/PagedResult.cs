namespace FlashSales.Domain.Results
{
    public sealed class PagedResult<T>(
        IReadOnlyCollection<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        public IReadOnlyCollection<T> Items { get; } = items;
        public int TotalCount { get; } = totalCount;
        public int Page { get; } = page;
        public int PageSize { get; } = pageSize;

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}