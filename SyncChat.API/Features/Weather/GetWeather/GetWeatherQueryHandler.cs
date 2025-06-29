using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Weather.GetWeather;

public class GetWeatherQueryHandler : IQueryHandler<GetWeatherQuery, int>
{
    public async Task<Result<int>> HandleAsync(GetWeatherQuery query, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return 3;
    }
}
