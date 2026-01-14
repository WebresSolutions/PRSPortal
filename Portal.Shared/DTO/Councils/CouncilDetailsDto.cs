using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Job;

namespace Portal.Shared.DTO.Councils;

public record CouncilDetailsDto(int councilId, string councilName, string phone, string email, string website, AddressDTO? address, List<ListJobDto> jobs);