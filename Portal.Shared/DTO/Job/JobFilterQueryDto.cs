namespace Portal.Shared.DTO.Job;

public record JobFilterDto(
    int Page = 1,
    int PageSize = 25,
    string? AddressSearch = null,
    string? ContactSearch = null,
    string? JobNumberSearch = null,
    string? OrderBy = null,
    SortDirectionEnum Order = SortDirectionEnum.Asc,
    bool Deleted = false,
    int? ContactId = null,
    int? CouncilId = null
);
