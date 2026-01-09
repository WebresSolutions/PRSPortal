namespace Portal.Shared.Web;

/// <summary>
/// Session data for the Facilities page
/// Stores pagination, search, and sorting state for the facilities/jobs listing
/// </summary>
public class SessionSearchData
{
    /// <summary>
    /// Gets or sets the current page number (0-based)
    /// </summary>
    public int Page { get; set; } = 0;
    /// <summary>
    /// Gets or sets the number of items per page
    /// </summary>
    public int PageSize { get; set; } = 25;
    /// <summary>
    /// Gets or sets the search filter string
    /// </summary>
    public string? SearchString { get; set; }
    /// <summary>
    /// Gets or sets the field name to order by
    /// </summary>
    public string? OrderBy { get; set; }
    /// <summary>
    /// Gets or sets the sort direction (ascending or descending)
    /// </summary>
    public SortDirectionEnum Order { get; set; } = SortDirectionEnum.Asc;
}