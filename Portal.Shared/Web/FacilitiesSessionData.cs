namespace Portal.Shared.Web;


/// <summary>
/// Session data for the Facilities page
/// </summary>
public class SessionSearchData
{
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 25;
    public string? SearchString { get; set; }
    public string? OrderBy { get; set; }
    public SortDirectionEnum Order { get; set; } = SortDirectionEnum.Asc;
}