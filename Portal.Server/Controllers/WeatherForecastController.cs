using Microsoft.AspNetCore.Mvc;
using Portal.Shared;

namespace Portal.Server.Controllers;

/// <summary>
/// API controller for weather forecast data
/// Provides sample weather forecast endpoints (typically used for testing)
/// </summary>
[ApiController, Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    /// <summary>
    /// Array of weather summary descriptions
    /// </summary>
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>
    /// Gets a collection of weather forecasts
    /// Returns sample forecast data for testing purposes
    /// </summary>
    /// <returns>A collection of weather forecast objects</returns>
    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        Random random = new();
        return [.. Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = random.Next(-20, 55),
            Summary = Summaries[random.Next(Summaries.Length)]
        })];
    }
}
