namespace  Portal.Shared.DTO.Quote;

public class  QuoteUpdateDto
{
    /// <summary>
    /// The unique identifier for the quote, which is required for quote updates. This field specifies the quote being updated and is essential for identifying and processing the update appropriately.
    /// </summary>
    public int QuoteId { get; set; }

    /// <summary>
    /// An optional reference for the quote, allowing users to provide a custom identifier or reference for the quote being updated
    /// </summary>
    public string? QuoteReferenceNumber { get; set; }
}