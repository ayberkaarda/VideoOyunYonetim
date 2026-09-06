using System.Collections.Generic;

namespace VideoGameManager.Data
{
    /// <summary>
    /// One page of results plus the size of the whole set.
    /// </summary>
    /// <typeparam name="T">Type of the listed item.</typeparam>
    /// <param name="Items">The rows on this page.</param>
    /// <param name="TotalCount">
    /// How many rows match the query in total, ignoring paging. Counted by a separate query
    /// that repeats the same filter.
    /// </param>
    /// <param name="Page">One-based number of the page that was read.</param>
    /// <param name="PageSize">How many rows a full page holds.</param>
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
    {
        /// <summary>
        /// How many pages the whole set spans. Zero when nothing matched.
        /// </summary>
        public int PageCount => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;

        /// <summary>
        /// <c>true</c> when at least one more page follows this one.
        /// </summary>
        public bool HasNextPage => Page < PageCount;
    }
}
