using Portal.Shared.DTO.Types;

namespace Portal.Shared.DTO.Quote;

public record QuoteHistoryDto(int quoteId, QuotesStatusTypeDto Status, DateTime DateChanged, string? ModifiedByUserName);