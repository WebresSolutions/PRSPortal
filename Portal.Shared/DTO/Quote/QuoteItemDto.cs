namespace Portal.Shared.DTO.Quote;

public class QuoteItemDto
{
    /// <summary>
    /// The unique identifier of the quote item. This ID is used to reference the specific item within a quote and can be used for operations such as updates or deletions of the item from the quote.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The identifier of the service associated with this quote item. This ID links the quote item to a specific service in the system, allowing for retrieval of service details and pricing information when processing the quote.
    /// </summary>
    public int ServiceTypeId { get; set; }
    /// <summary>
    /// Gets or sets the name of the service associated with this instance.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    /// <summary>
    /// The total price for this quote item. This value is calculated based on the service type and any additional factors such as quantity or discounts, and it contributes to the overall total price of the quote.
    /// </summary>
    public decimal Total { get; set; }
    /// <summary>
    /// Optional notes or comments related to this quote item. This field can be used to provide additional information or special instructions regarding the service being quoted, which can be helpful for both the client and the service provider when reviewing the quote details.
    /// </summary>
    public string? Notes { get; set; }

}