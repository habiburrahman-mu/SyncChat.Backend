using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SyncChat.API.Infrastructure.Socket;

public sealed class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Assumes "sub" claim holds the user ID
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? connection.User?.FindFirst("sub")?.Value;
    }
}
