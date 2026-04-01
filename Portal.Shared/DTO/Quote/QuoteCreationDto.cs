using System.ComponentModel.DataAnnotations;
using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Quote;

public class QuoteCreationDto
{
    /// <summary>
    /// The unique identifier for the quote type, which is required for quote creation. This field specifies the type of quote being created and is essential for categorizing and processing the quote appropriately.
    /// </summary>
    public int QuoteTypeId { get; set; }
    /// <summary>
    /// The unique identifier for the quote status, which is required for quote creation. This field specifies the current status of the quote being created and is essential for tracking and managing the quote throughout its lifecycle.
    /// </summary>
    public int QuoteStatusId { get; set; }
    /// <summary>
    /// The Id of the contact associated with the quote, which is required for quote creation. 
    /// This field links the quote to a specific contact, allowing for better organization and communication regarding the quote.
    /// Notifications will be sent to the contact associated with the quote, ensuring that they are kept informed about the status and any updates related to the quote.
    /// </summary>
    public int ContactId { get; set; }
    /// <summary>
    /// An optional reference for the quote, allowing users to provide a custom identifier or reference for the quote being created
    /// </summary>
    public string? QuoteReference { get; set; }
    /// <summary>
    /// An optional description for the quote, providing additional details or context about the quote being created
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The target delivery date for the quote, which is optional during quote creation. This field indicates the expected delivery date for the services or products associated with the quote, allowing for better planning and scheduling.
    /// </summary>
    public DateTime? TargetDeliveryDate { get; set; }

    /// <summary>
    /// The address associated with the quote, which is required for quote creation
    /// </summary>
    [Required]
    public required AddressDto Address { get; set; }

    public List<QuoteItemDto> QuoteItems { get; set; } = [];
}
