namespace Portal.Shared.Web;

/// <summary>
/// Data model for Quick Search functionality
/// Contains all search criteria fields for quick searching across jobs
/// </summary>
public class QuickSearchData
{
    /// <summary>
    /// Gets or sets the contact name to search for
    /// </summary>
    public string? Contact { get; set; }

    /// <summary>
    /// Gets or sets the address/street to search for
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the suburb to search for
    /// </summary>
    public string? Suburb { get; set; }

    /// <summary>
    /// Gets or sets the cadastral number to search for
    /// </summary>
    public string? CadastralNo { get; set; }

    /// <summary>
    /// Gets or sets the setout number to search for
    /// </summary>
    public string? SetoutNo { get; set; }

    /// <summary>
    /// Clears all search fields
    /// </summary>
    public void Clear()
    {
        Contact = null;
        Address = null;
        Suburb = null;
        CadastralNo = null;
        SetoutNo = null;
    }

    /// <summary>
    /// Checks if any search criteria has been entered
    /// </summary>
    /// <returns>True if at least one field has a value, otherwise false</returns>
    public bool HasSearchCriteria()
    {
        return !string.IsNullOrWhiteSpace(Contact) ||
               !string.IsNullOrWhiteSpace(Address) ||
               !string.IsNullOrWhiteSpace(Suburb) ||
               !string.IsNullOrWhiteSpace(CadastralNo) ||
               !string.IsNullOrWhiteSpace(SetoutNo);
    }
}

