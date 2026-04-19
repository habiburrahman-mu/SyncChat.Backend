using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Notification.Contracts;

namespace SyncChat.API.Infrastructure.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailSettings emailSettings;
    private readonly ILogger<SmtpEmailService> logger;

    public SmtpEmailService(
        IOptions<EmailSettings> emailOptions,
        ILogger<SmtpEmailService> logger)
    {
        this.emailSettings = emailOptions.Value;
        this.logger = logger;
    }

    public async Task SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(emailSettings.From));
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;

        var builder = new BodyBuilder
        {
            HtmlBody = request.Body,
            TextBody = StripHtml(request.Body)
        };

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(emailSettings.SmtpHost, emailSettings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(emailSettings.SmtpUser, emailSettings.SmtpPass, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("Sent email to {Email}", request.To);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var array = new char[html.Length];
        int arrayIndex = 0;
        bool inside = false;

        foreach (char @let in html)
        {
            if (@let == '<')
            {
                inside = true;
                continue;
            }
            if (@let == '>')
            {
                inside = false;
                continue;
            }
            if (!inside)
            {
                array[arrayIndex++] = @let;
            }
        }

        return new string(array, 0, arrayIndex);
    }
}
