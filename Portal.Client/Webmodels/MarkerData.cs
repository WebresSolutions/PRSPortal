using GoogleMapsComponents.Maps;

namespace Portal.Client.Webmodels;


public class MarkerData
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public bool Clickable { get; set; } = true;
    public bool Draggable { get; set; }
    public bool Active { get; set; }

    public void UpdatePosition(LatLngLiteral position)
    {
        Lat = position.Lat;
        Lng = position.Lng;
    }
}

