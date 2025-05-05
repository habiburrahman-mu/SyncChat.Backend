using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Weather.GetWeather;

public class GetWeatherEndpoint : IWeatherEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(WeatherRoute.GetWeatherForecast, async (IQuerySender sender) =>
        {
            return await sender.SendAsync(new GetWeatherQuery());
        })
        .WithName("GetWeatherForecast");
    }
}