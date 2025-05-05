using SyncChat.API.Routing;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Weather;

[RouteGroupPrefix(WeatherRoute.Base, "Weather")]
public interface IWeatherEndpoint : IEndpoint { }
