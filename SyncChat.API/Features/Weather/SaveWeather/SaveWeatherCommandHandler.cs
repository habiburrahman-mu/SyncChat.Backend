using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Weather.SaveWeather;

public class SaveWeatherCommandHandler : ICommandHandler<SaveWeatherCommand>
{
    public Task HandleAsync(SaveWeatherCommand command, CancellationToken cancellationToken = default)
    {

        return Task.CompletedTask;
    }
}
