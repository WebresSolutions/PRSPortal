namespace Portal.Shared.DTO.Address;

public class AddressDTO
{
    public int AddressId { get; set; }
    public StateEnum? State { get; set; }
    public int StateId { get; set; }
    public string Suburb { get; set; }
    public string Street { get; set; }
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
}

public class LatLngDto
{
    public double Latitude { get; set; }
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