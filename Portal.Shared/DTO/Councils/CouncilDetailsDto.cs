using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Councils;

public record CouncilDetailsDto(
    int CouncilId,
    string CouncilName,
    string Phone,
    string? Fax,
    string Email,
    string Website,
    AddressDTO? Address,
    int JobCount,
    int ContactCount);