using Portal.Shared.DataEnums;

namespace Portal.Shared.DTO.Types;

public record QuotesStatusTypeDto(QuoteStatusEnum StatusEnum, string Name, string Colour, bool IsActive);
