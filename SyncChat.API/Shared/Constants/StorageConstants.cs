namespace SyncChat.API.Shared.Constants;

public static class StorageConstants
{
    public static class MediaKeys
    {
        public const string Prefix = "media/";

        public static string For(Guid mediaId) => $"{Prefix}{mediaId}";
    }
}
