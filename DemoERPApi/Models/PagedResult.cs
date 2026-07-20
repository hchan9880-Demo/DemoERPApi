namespace DemoERPApi.Models;


/// Generic paged result model for paginated API responses.

/// <typeparam name="T">Type of items in the result.</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }

    public int TotalPages =>
        TotalRecords == 0 ? 0 : (int)Math.Ceiling(TotalRecords / (double)PageSize);
}