using GoogleMapsComponents;
using GoogleMapsComponents.Maps;
using Microsoft.AspNetCore.Components;
using Portal.Client.Webmodels;
using Portal.Shared.DTO.Address;

namespace Portal.Client.Pages.Job;

public partial class EditAddress : IDisposable
{
    [Parameter]
    public required AddressDTO Address { get; set; }

    /// <summary>
    /// the list of active markers on the map
    /// </summary>
    private readonly List<MarkerData> Markers = [];
    /// <summary>
    /// Marker clustering instance
    /// </summary>
    private AdvancedGoogleMap? Map;

    private MapOptions MapOptions = default!;

    protected override async Task OnInitializedAsync()
    {
        if (Address.LatLng is null)
            Address.LatLng = new()
            {
                Latitude = -37.8136,
                Longitude = 144.9631
            };

        Markers.Add(new MarkerData { Id = 1, Lat = Address.LatLng!.Latitude, Lng = Address.LatLng!.Longitude, Title = "New", Draggable = true });
        MapOptions = new MapOptions
        {
            Zoom = 10,
            Center = new LatLngLiteral(-37.8136, 144.9631),
            MapTypeId = MapTypeId.Roadmap,
            MapId = "Single_map_id", // Required for advanced markers
        };
    }

    /// <summary>
    /// Helper to unpack the event data and call the main logic.
    /// </summary>
    private async Task HandleDragEnd(LatLngLiteral pos)
    {
        Address?.LatLng?.Latitude = pos.Lat;
        Address?.LatLng?.Longitude = pos.Lng;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _ = Map?.DisposeAsync();
    }
}