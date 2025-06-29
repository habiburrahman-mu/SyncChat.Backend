using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Weather.SaveWeather;

public class SaveWeatherCommandHandler : ICommandHandler<SaveWeatherCommand>
{
    public async Task<Result> HandleAsync(SaveWeatherCommand command, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return Result.Success();
    }
}
