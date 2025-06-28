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

    public static class UserRoute
    {
        public const string Base = "user";
        public const string GetUserByUserName = "/getByUserName";
    }

    public static class ConversationRoute
    {
        public const string Base = "conversation";
        public const string GetConversationsByUserId = "/getByUserId";
        public const string GetConversationById = "/getById";
        public const string SendMessage = "/sendMessage";
        //public const string UpdateConversation = "/update";
        //public const string DeleteConversation = "/delete";
    }
}
