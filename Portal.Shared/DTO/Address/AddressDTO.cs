namespace Portal.Shared.DTO.Address;

public record AddressDTO(
    int addressId,
    StateEnum? state,
    int stateId,
    string suburb,
    string street,
    string postCode
    );
