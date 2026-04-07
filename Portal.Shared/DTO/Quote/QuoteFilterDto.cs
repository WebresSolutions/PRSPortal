namespace Portal.Shared.DTO.Quote;

public record QuoteFilterDto(
    int Page = 1,
    int PageSize = 25,
    string? JobNumberSearch = null,
    string? ContactSearch = null,
    string? AddressSearch = null,
    string? OrderBy = null,
    SortDirectionEnum Order = SortDirectionEnum.Asc,
    bool ShowDeleted = false
);