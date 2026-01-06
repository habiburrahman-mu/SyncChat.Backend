using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using SyncChat.API.Shared.Configuration;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace SyncChat.API.Infrastructure.AuthProviders.Google;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo> ValidateAsync(string token);
}

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthSettings googleAuthSettings;

    public GoogleTokenValidator(IOptions<GoogleAuthSettings> googleAuthOptions)
    {
        this.googleAuthSettings = googleAuthOptions.Value;
    }

    public async Task<GoogleUserInfo> ValidateAsync(string token)
    {
        var validationSettings = new ValidationSettings
        {
            Audience = new[] { this.googleAuthSettings.ClientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(token, validationSettings);

        return new GoogleUserInfo(
            Subject: payload.Subject,
            Email: payload.Email,
            EmailVerified: payload.EmailVerified,
            Name: payload.Name,
            GivenName: payload.GivenName,
            FamilyName: payload.FamilyName,
            Picture: payload.Picture,
            Locale: payload.Locale);
    }
}
