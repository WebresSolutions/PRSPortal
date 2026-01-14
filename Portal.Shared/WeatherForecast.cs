namespace Portal.Shared;

/// <summary>
/// Represents a weather forecast data model
/// Used for sample/testing purposes in the application
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// Gets or sets the date of the forecast
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the temperature in Celsius
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// Gets or sets the weather summary description
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// Gets the temperature in Fahrenheit
    /// Calculated from the Celsius temperature
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
