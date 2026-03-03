using GoogleMapsComponents.Maps;

namespace Portal.Client.Webmodels;

/// <summary>
/// Represents a map marker with position, title, and drag/click options.
/// </summary>
public class MarkerData
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public bool Clickable { get; set; } = true;
    public bool Draggable { get; set; }
    public bool Active { get; set; }

    /// <summary>
    /// Updates the marker's latitude and longitude from the given position.
    /// </summary>
    /// <param name="position">The new map position.</param>
    public void UpdatePosition(LatLngLiteral position)
    {
        Lat = position.Lat;
        Lng = position.Lng;
    }
}

