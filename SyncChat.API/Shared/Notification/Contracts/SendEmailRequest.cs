namespace SyncChat.API.Shared.Notification.Contracts;

public record SendEmailRequest(string To, string Subject, string Body);
