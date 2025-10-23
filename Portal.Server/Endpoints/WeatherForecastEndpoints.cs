using Portal.Shared;

namespace Portal.Server.Endpoints;


public static class WeatherForecastEndpoints
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public static void MapWeatherForecastEndpoints(this WebApplication app)
    {
        _ = app.MapGet("/weatherforecast", Get)
            .WithName("GetWeatherForecast")
            .WithTags("WeatherForecast");
    }

    public static IEnumerable<WeatherForecast> Get()
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
