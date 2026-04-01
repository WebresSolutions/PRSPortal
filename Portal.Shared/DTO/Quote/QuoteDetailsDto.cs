using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Types;

namespace Portal.Shared.DTO.Quote;

public record QuoteDetailsDto(int Id, string QuoteReference, string Description, int QuoteTypeId, QuotesStatusTypeDto QuotesStatus, AddressDto Address, ListContactDto Contact, Decimal TotalPrice, DateTime CreatedAt, DateTime UpdatedAt, int? JobId, string? JobNumber);