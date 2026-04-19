using SyncChat.API.Shared.Events;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Notification.Contracts;
using Microsoft.Extensions.Options;
using SyncChat.API.Shared.Configuration;

namespace SyncChat.API.Infrastructure.Notification;

public sealed class SendPasswordResetEmailEventHandler : IDomainEventHandler<SendPasswordResetEmailEvent>
{
    private readonly IEmailService emailService;
    private readonly IPasswordResetTokenService passwordResetTokenService;
    private readonly JWTSettings jwtSettings;
    private readonly ILogger<SendPasswordResetEmailEventHandler> logger;

    public SendPasswordResetEmailEventHandler(
        IEmailService emailService,
        IPasswordResetTokenService passwordResetTokenService,
        IOptions<JWTSettings> jwtOptions,
        ILogger<SendPasswordResetEmailEventHandler> logger)
    {
        this.emailService = emailService;
        this.passwordResetTokenService = passwordResetTokenService;
        this.jwtSettings = jwtOptions.Value;
        this.logger = logger;
    }

    public async Task HandleAsync(SendPasswordResetEmailEvent domainEvent, CancellationToken cancellationToken)
    {
        string token = passwordResetTokenService.CreateToken(domainEvent.PasswordResetTokenId);
        string urlBase = jwtSettings.PasswordResetUrlBase ?? throw new InvalidOperationException("PasswordResetUrlBase is not configured in JWT settings.");
        string encodedToken = Uri.EscapeDataString(token);
        string resetUrl = urlBase.Contains("?") ? $"{urlBase}&token={encodedToken}" : $"{urlBase}?token={encodedToken}";

        int expirationMinutes = jwtSettings.PasswordResetExpirationInMinutes;

        string body = $"<p>Hi {HtmlEncode(domainEvent.FullName)},</p>" +
            $"<p>We received a request to reset your SyncChat password. Click the button below to continue:</p>" +
            $"<p><a href=\"{resetUrl}\" style=\"display:inline-block;padding:12px 18px;background:#0078d4;color:#ffffff;text-decoration:none;border-radius:6px;\">Reset password</a></p>" +
            $"<p>If the button does not work, paste this link into your browser:</p>" +
            $"<p>{HtmlEncode(resetUrl)}</p>" +
            $"<p>This link expires in {expirationMinutes} minutes and can only be used once.</p>" +
            "<p>If you did not request a password reset, please ignore this email.</p>";

        try
        {
            await emailService.SendEmailAsync(new SendEmailRequest(domainEvent.To, "Reset your SyncChat password", body), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email to {Email}", domainEvent.To);
            throw;
        }
    }

    private static string HtmlEncode(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return System.Net.WebUtility.HtmlEncode(text);
    }
}
