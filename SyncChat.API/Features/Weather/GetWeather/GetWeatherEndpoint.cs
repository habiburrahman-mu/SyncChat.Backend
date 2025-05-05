using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Weather.GetWeather;

public class GetWeatherEndpoint : IWeatherEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async (IQuerySender sender) =>
        {
            return await sender.SendAsync(new GetWeatherQuery());
        })
        .WithName("GetWeatherForecast");
    }
}