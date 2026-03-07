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
    public string? AddressSearch { get; set; }
    /// <summary>
    /// Contact Search. Search by name, phone and email
    /// </summary>
    public string? ContactSearch { get; set; }
    /// <summary>
    /// Job number search string
    /// </summary>
    public string? JobNumberSearch { get; set; }
    /// <summary>
    /// Search String (legacy / type-ahead)
    /// </summary>
    public string? SearchString { get; set; }
    /// <summary>
    /// Contact name search (contacts page)
    /// </summary>
    public string? NameSearch { get; set; }
    /// <summary>
    /// Contact email search (contacts page)
    /// </summary>
    public string? EmailSearch { get; set; }
    /// <summary>
    /// Contact phone search (contacts page)
    /// </summary>
    public string? PhoneSearch { get; set; }
    /// <summary>
    /// Contact ID search (contacts page). AddressSearch (above) is also used for contacts page address search.
    /// </summary>
    public string? ContactIdSearch { get; set; }
    /// <summary>
    /// Gets or sets the field name to order by
    /// </summary>
    public string? OrderBy { get; set; }
    /// <summary>
    /// Gets or sets the sort direction (ascending or descending)
    /// </summary>
    public SortDirectionEnum Order { get; set; } = SortDirectionEnum.Asc;
    /// <summary>
    /// Show deleted facilities/jobs in the listing when true. Defaults to false to hide deleted items.
    /// </summary>
    public bool ShowDeleted { get; set; } = false;
}