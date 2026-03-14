namespace SyncChat.API.Shared.Constants;

public static class StorageConstants
{
    public static class MediaKeys
    {
        public const string Prefix = "media/";

        public static string For(Guid mediaId) => $"{Prefix}{mediaId}";
    }

    public static class AvatarKeys
    {
        public const string Prefix = "avatars/";

        public static string For(Guid userUUID) => $"{Prefix}{userUUID}";
    }

    public static class MinioClientKeys
    {
        public const string Internal = "minio-internal";
        public const string Presign = "minio-presign";
    }
}
