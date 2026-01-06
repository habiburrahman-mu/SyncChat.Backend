namespace SyncChat.API.Infrastructure.AuthProviders.Google;

public record GoogleUserInfo(
    string Subject,
    string Email,
    bool EmailVerified,
    string Name,
    string GivenName,
    string FamilyName,
    string Picture,
    string Locale);