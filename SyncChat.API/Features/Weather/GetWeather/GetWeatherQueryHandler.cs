using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Weather.GetWeather;

public class GetWeatherQueryHandler : IQueryHandler<GetWeatherQuery, int>
{
    public Task<int> HandleAsync(GetWeatherQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(3);
    }
}
