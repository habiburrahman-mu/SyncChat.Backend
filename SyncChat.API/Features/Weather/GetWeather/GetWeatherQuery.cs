using SyncChat.API.Features.Weather.SaveWeather;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Weather.GetWeather;

public class GetWeatherQuery : IQuery
{
}

public class GetWeatherCommandHandler : IQueryHandler<GetWeatherQuery>
{
    public Task HandleAsync(GetWeatherQuery command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(3);
    }
}