using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Types;

namespace Portal.Shared.DTO.Quote;

public record QuoteDetailsDto(
    int Id,
    string QuoteReference,
    string? Description,
    decimal TotalPrice,
    QuoteItemDto[] QuoteItems,
    int QuoteTypeId,
    QuotesStatusTypeDto QuotesStatus,
    AddressDto? Address,
    ListContactDto? Contact,
    DateTime CreatedAt,
    string Createdby,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    DateTime? TargetDeliveryDate,
    DateTime? DateSentToClient,
    int? JobId,
    string? JobNumber);