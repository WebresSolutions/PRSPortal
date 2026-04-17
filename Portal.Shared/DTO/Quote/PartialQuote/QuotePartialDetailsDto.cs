using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;

namespace Portal.Shared.DTO.Quote.PartailQuote;

/// <param name="QuoteRef"> The quote reference </param>
/// <param name="Total"> The total cost of the quote </param>
/// <param name="Contact"> Contains contact details </param>
/// <param name="LineItems"> List of line items </param>
public record QuotePartialDetailsDto(QuoteStatusEnum QuoteStatus, DateTime ValidUntilDate, string QuoteRef, decimal Total, ListContactDto Contact, AddressDto Address, QuoteLineItemPartialDto[] LineItems);
