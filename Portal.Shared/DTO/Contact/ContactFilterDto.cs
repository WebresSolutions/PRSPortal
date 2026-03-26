namespace Portal.Shared.DTO.Contact;

/// <summary>
/// Filter parameters for the contacts list API.
/// Supports split search fields (name, email, phone, address, contactId) or a single searchFilter for type-ahead.
/// </summary>
public record ContactFilterDto(
    int Page = 1,
    int PageSize = 25,
    string? OrderBy = null,
    SortDirectionEnum Order = SortDirectionEnum.Asc,
    bool Deleted = false,
    string? NameEmailPhoneSearch = null,
    string? AddressSearch = null,
    string? SearchFilter = null,
    ContactTypeEnum? contactType = null
);
