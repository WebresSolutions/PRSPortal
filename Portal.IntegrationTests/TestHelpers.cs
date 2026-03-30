using Portal.Shared.DTO.Address;

namespace Portal.IntegrationTests;

internal static class TestHelpers
{
    public static void AssertAddressAreSame(AddressDTO addressDTO, AddressDTO otherAddressDTO)
    {
        Assert.NotNull(addressDTO);
        Assert.NotNull(otherAddressDTO);
        Assert.Equal(addressDTO.Street, otherAddressDTO.Street);
        Assert.Equal(addressDTO.Suburb, otherAddressDTO.Suburb);
        Assert.Equal(addressDTO.State, otherAddressDTO.State);
        Assert.Equal(addressDTO.LatLng?.Latitude, otherAddressDTO.LatLng?.Latitude);
        Assert.Equal(addressDTO.LatLng?.Longitude, otherAddressDTO.LatLng?.Longitude);
        Assert.Equal(addressDTO.PostCode, otherAddressDTO.PostCode);
    }

}
