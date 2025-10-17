namespace Portal.Shared.ResponseModels;

/// <summary>
/// Generic class for representing paginated data responses
/// Provides pagination information along with the current page of results
/// </summary>
/// <typeparam name="T">The type of items contained in the response</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// Initializes a new instance of the PagedResponse class
    /// </summary>
    public PagedResponse()
    { }

    /// <summary>
    /// Initializes a new instance of the PagedResponse class with data and pagination parameters
    /// </summary>
    /// <param name="values">The collection of items for the current page</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="currentPage">The current page number (1-based)</param>
    /// <param name="totalItems">The total number of items across all pages</param>
    public PagedResponse(IEnumerable<T> values, int pageSize, int currentPage, int totalItems)
    {
        this.Result = [.. values];
        this.PageSize = pageSize;
        this.CurrentPage = currentPage;
        this.TotalCount = totalItems;
        TotalNumberPages = pageSize > 0 ? totalItems / pageSize : 1;
    }

    /// <summary>
    /// Gets or sets the list of items for the current page
    /// </summary>
    public List<T> Result { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages available
    /// </summary>
    public int TotalNumberPages { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the current page number (1-based)
    /// </summary>
    public int CurrentPage { get; init; }
}