using SyncChat.API.Features.Weather.SaveWeather;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Weather.GetWeather;

public record GetWeatherQuery : IQuery<int> {}

public class GetWeatherQueryHandler : IQueryHandler<GetWeatherQuery, int>
{
    public Task<int> HandleAsync(GetWeatherQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(3);
    }
}