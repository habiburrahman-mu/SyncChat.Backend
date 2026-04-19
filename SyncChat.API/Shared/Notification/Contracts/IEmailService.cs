namespace SyncChat.API.Shared.Notification.Contracts;

public interface IEmailService
{
    Task SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken);
}
