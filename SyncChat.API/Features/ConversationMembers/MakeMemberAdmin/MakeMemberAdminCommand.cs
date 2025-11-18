using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using SyncChat.API.Features.ConversationMembers.RemoveMemberFromConversation;
using SyncChat.API.Features.Notifications;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.ConversationMembers.MakeMemberAdmin;

public sealed record MakeMemberAdminCommand(
    long ConversationMemberId) : ICommand;

public sealed class MakeMemberAdminCommandHandler : ICommandHandler<MakeMemberAdminCommand>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IIdentityService identityService;
    private readonly IHubContext<NotificationHub, INotificationClient> hub;
    private readonly ILogger<RemoveConversationMemberCommandHandler> logger;

    public MakeMemberAdminCommandHandler(
        ApplicationDbContext dbContext,
        IIdentityService identityService,
        IHubContext<NotificationHub, INotificationClient> hub,
        ILogger<RemoveConversationMemberCommandHandler> logger)
    {
        this.dbContext = dbContext;
        this.identityService = identityService;
        this.hub = hub;
        this.logger = logger;
    }

    public Task<Result> HandleAsync(MakeMemberAdminCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public sealed class MakeMemberAdminCommandValidator : AbstractValidator<MakeMemberAdminCommand>
{
    public MakeMemberAdminCommandValidator()
    {
        
    }
}