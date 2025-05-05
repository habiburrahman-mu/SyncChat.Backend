using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Weather.SaveWeather;

public class SaveWeatherEndpoint : IWeatherEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(WeatherRoute.SaveWeatherForecast, async (ICommandSender sender) =>
        {
            await sender.SendAsync(new SaveWeatherCommand());
        })
        .WithSummary("SaveWeatherForecast");
    }
}