namespace SyncChat.API.Shared.Constants;

public class EndpointConstants
{
    public static class HubRoute
    {
        public const string NotificationHub = "/hub/notifications";
    }

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

    public static class UserRoute
    {
        public const string Base = "user";
        public const string GetUserByUserName = "/getByUserName";
    }

    public static class ConversationRoute
    {
        public const string Base = "conversation";
        public const string GetList = "/getList";
        public const string Create = "/create";
        public const string GetLastMessage = "/getLastMessage";
        public const string MarkMessageAsSeen = "/MarkMessageAsSeen";
    }

    public static class MessageRoute
    {
        public const string Base = "message";
        public const string GetList = "/getList";
        public const string Send = "/send";
    }
}
