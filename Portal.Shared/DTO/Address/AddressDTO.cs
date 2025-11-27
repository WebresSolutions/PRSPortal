namespace Portal.Shared.DTO.Address;

public record AddressDTO(
    int AddressId,
    StateEnum? State,
    int StateId,
    string suburb,
    string street,
    string postCode
    );
