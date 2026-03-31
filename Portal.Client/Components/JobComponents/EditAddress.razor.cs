using GoogleMapsComponents;
using GoogleMapsComponents.Maps;
using Microsoft.AspNetCore.Components;
using Portal.Client.Webmodels;
using Portal.Shared.DTO.Address;

namespace Portal.Client.Components.JobComponents;

public partial class EditAddress : IDisposable
{
    [Parameter]
    public required AddressDto Address { get; set; }

    /// <summary>
    /// the list of active markers on the map
    /// </summary>
    private readonly List<MarkerData> _markers = [];
    /// <summary>
    /// Marker clustering instance
    /// </summary>
    private AdvancedGoogleMap? _map;

    private MapOptions _mapOptions = default!;

    /// <summary>
    /// Initializes the map with default or existing address coordinates and adds a draggable marker.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        if (Address.LatLng is null)
            Address.LatLng = new()
            {
                Latitude = -37.8136,
                Longitude = 144.9631
            };

        _markers.Add(new MarkerData { Id = 1, Lat = Address.LatLng!.Latitude, Lng = Address.LatLng!.Longitude, Title = "New", Draggable = true });
        _mapOptions = new MapOptions
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

    /// <summary>
    /// Releases the map component resources when the component is disposed.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _ = _map?.DisposeAsync();
    }
}