using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Address;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.Quote;

public class QuoteUpdateDto
{
    /// <summary>
    /// The unique identifier for the quote, which is required for quote updates. This field specifies the quote being updated and is essential for identifying and processing the update appropriately.
    /// </summary>
    public int QuoteId { get; set; }

    /// <summary>
    /// An optional reference for the quote, allowing users to provide a custom identifier or reference for the quote being updated
    /// </summary>
    public string? QuoteReferenceNumber { get; set; }

    /// <summary>
    /// An optional description for the quote, providing additional details or context about the quote being updated
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// The status of the quote, which is required for quote updates
    /// </summary>
    public required QuoteStatusEnum QuoteStatusId { get; set; }

    /// <summary>
    /// The type of the quote
    /// </summary>
    public required JobTypeEnum JobType { get; set; }

    /// <summary>
    /// Contact ID, which is required for quote updates. This field links the quote to a sspecific contact, allowing for better organization and communication regarding the quote.k
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Contact ID must be at least 1")]
    public int ContactId { get; set; }

    /// <summary>
    /// The target delivery date for the quote, which is optional during quote updates
    /// </summary>
    public DateTime? TargetDeliveryDate { get; set; }

    /// <summary>
    /// The address associated with the quote, which is required for quote updates
    /// </summary>
    [Required]
    public required AddressDto Address { get; set; }

    /// <summary>
    /// The items of the quote, which are required for quote updates
    /// </summary>
    [Required]
    public List<QuoteItemDto> QuoteItems { get; set; } = [];

}