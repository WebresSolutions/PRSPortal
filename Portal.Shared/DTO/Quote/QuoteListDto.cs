using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Types;

namespace Portal.Shared.DTO.Quote;

/// <summary>
/// Data transfer object representing a quote in a list format
/// Contains the unique identifier and color associated with the quote
/// </summary>
/// <param name="Id"></param>
/// <param name="QuoteReference"></param>
/// <param name="Status"></param>
public record QuoteListDto(int Id, string QuoteReference, decimal? TotalPrice, QuotesStatusTypeDto Status, ContactDto? Contact, AddressDto? Address, int? JobId, string? JobNumber);
