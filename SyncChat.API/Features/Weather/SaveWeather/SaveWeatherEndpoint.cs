using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Weather.SaveWeather;

public class SaveWeatherEndpoint : IWeatherEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async (ICommandSender sender) =>
        {
            await sender.SendAsync(new SaveWeatherCommand());
        })
        .WithName("SaveWeatherForecast");
    }
}