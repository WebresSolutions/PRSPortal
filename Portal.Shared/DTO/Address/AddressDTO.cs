using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.Address;

public class AddressDTO
{
    public int AddressId { get; set; }
    public StateEnum? State { get; set; }
    public int StateId { get; set; }

    [MaxLength(200, ErrorMessage = "Suburb cannot exceed 200 characters")]
    public string Suburb { get; set; }

    [MaxLength(500, ErrorMessage = "Street cannot exceed 500 characters")]
    public string Street { get; set; }

    [MaxLength(10, ErrorMessage = "Post code cannot exceed 10 characters")]
    [RegularExpression(@"^(\d{4})?$", ErrorMessage = "Post code must be 4 digits when provided (e.g. 3000)")]
    public string PostCode { get; set; }

    public LatLngDto? LatLng { get; set; }

    // Empty Constructor
    public AddressDTO()
    {
        Suburb = "";
        Street = "";
        PostCode = "";
    }

    // Primary Constructor
    public AddressDTO(
        int addressId,
        StateEnum? state,
        int stateId,
        string suburb,
        string street,
        string postCode,
        LatLngDto? latLng = null)
    {
        AddressId = addressId;
        State = state;
        StateId = stateId;
        Suburb = suburb;
        Street = street;
        PostCode = postCode;
        LatLng = latLng;
    }

    /// <summary>
    /// Returns a formatted display string (suburb, state, post code) for the address.
    /// Skips empty parts and returns "No address" when nothing is set.
    /// </summary>
    /// <returns>The formatted address string or "No address" if all parts are empty.</returns>
    public string ToDisplayString()
    {
        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(Street))
            parts.Add(Street.ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(Suburb))
            parts.Add(Suburb.ToUpperInvariant());
        if (State is not null && !string.IsNullOrWhiteSpace(State.ToString()))
            parts.Add(State.ToString()!);
        if (!string.IsNullOrWhiteSpace(PostCode))
            parts.Add(PostCode);
        return parts.Count > 0 ? string.Join(" ", parts) : "No address";
    }
}

public class LatLngDto
{
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Longitude { get; set; }

    // Empty Constructor
    public LatLngDto() { }

    // Primary Constructor
    public LatLngDto(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}