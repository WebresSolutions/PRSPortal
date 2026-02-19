namespace Portal.Shared.DTO.Address;

/// <summary>
/// Data transfer object representing address information
/// Contains address details including street, suburb, postcode, and state
/// </summary>
/// <param name="AddressId">The unique identifier for the address</param>
/// <param name="State">The state enumeration value</param>
/// <param name="StateId">The numeric identifier for the state</param>
/// <param name="Suburb">The suburb or city name</param>
/// <param name="Street">The street address</param>
/// <param name="PostCode">The postal code</param>
public record AddressDTO(
    int AddressId,
    StateEnum? State,
    int StateId,
    string Suburb,
    string Street,
    string PostCode,
    LatLngDto? LatLng = null
    );

public record LatLngDto(
    double Latitude,
    double Longitude
    );
