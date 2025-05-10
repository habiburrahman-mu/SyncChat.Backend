namespace SyncChat.API.Shared.Constants;

public class EndpointConstants
{
    public static class AuthRoute
    {
        public const string Base = "auth";
        public const string Register = "/register";
        public const string Token = "/token";
    }

    public static class WeatherRoute
    {
        public const string Base = "weatherforecast";

        public const string GetWeatherForecast = "/";
        public const string SaveWeatherForecast = "/";
    }


}
