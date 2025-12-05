using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.ConversationMembers.RemoveMemberFromConversation;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Features.Notifications;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Utilities;

namespace SyncChat.API.Features.ConversationMembers.MakeMemberAdmin;

public sealed record MakeMemberAdminCommand(
    long ConversationMemberId) : ICommand;

public sealed class MakeMemberAdminCommandHandler : ICommandHandler<MakeMemberAdminCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IHubContext<NotificationHub, INotificationClient> _hub;
    private readonly ILogger<RemoveConversationMemberCommandHandler> _logger;

    public MakeMemberAdminCommandHandler(
        ApplicationDbContext dbContext,
        IIdentityService identityService,
        IHubContext<NotificationHub, INotificationClient> hub,
        ILogger<RemoveConversationMemberCommandHandler> logger)
    {
        this._dbContext = dbContext;
        this._identityService = identityService;
        this._hub = hub;
        this._logger = logger;
    }

    public async Task<Result> HandleAsync(MakeMemberAdminCommand command, CancellationToken cancellationToken = default)
    {
        var conversationMember = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(x => x.MemberId == command.ConversationMemberId, cancellationToken);

        if (conversationMember is null)
            return Result.Failure(ConversationMemberErrors.NotFound(command.ConversationMemberId));

        // Check if user has necessary permissions
        long actorId = _identityService.GetUserID();
        ConversationMember? actorMembership = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(x => x.UserId == actorId && x.ConversationId == conversationMember.ConversationId, cancellationToken);

        if (actorMembership is null || !HasAdminPromotePermission(actorMembership.Role))
            return Result.Failure(ConversationMemberErrors.Forbidden());

        // Check if member is already an admin or owner
        if (conversationMember.Role == MemberRole.Admin || conversationMember.Role == MemberRole.Owner)
            return Result.Failure(ConversationMemberErrors.AlreadyAdmin(command.ConversationMemberId));

        // Update member role to admin
        conversationMember.Role = MemberRole.Admin;
        _dbContext.ConversationMembers.Update(conversationMember);

        _dbContext.ConversationMembers.Update(conversationMember);

        Message systemMessage = await AddSystemMessageAsync(conversationMember, actorId, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await SendNotificationAsync(conversationMember, systemMessage, cancellationToken);

        return Result.Success();
    }

    private async Task<Message> AddSystemMessageAsync(ConversationMember member, long actorId, CancellationToken cancellationToken)
    {
        Message message = new()
        {
            Uuid = Guid.NewGuid(),
            ConversationId = member.ConversationId,
            SenderId = actorId,
            Type = MessageType.System,
            MetaData = SystemMessageHelper.MemberPromotedToAdmin(member.UserId, actorId),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _dbContext.Messages.AddAsync(message, cancellationToken);

        Conversation conversation = await _dbContext.Conversations
            .FirstAsync(x => x.ConversationId == member.ConversationId, cancellationToken);

        conversation.LastMessage = message;
        _dbContext.Conversations.Update(conversation);

        return message;
    }

    private async Task SendNotificationAsync(ConversationMember promotedMember, Message systemMessage, CancellationToken cancellationToken)
    {
        try
        {
            // Get all active members
            List<long> memberIds = await _dbContext.ConversationMembers
                .Where(x => x.ConversationId == systemMessage.ConversationId
                        && x.IsActive
                        && x.LeftAt == null)
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

            // Create all notification tasks in a single concatenated collection
            var allNotificationTasks = new[]
            {
                // Notify the promoted member that they were made admin
                _hub.Clients.User(promotedMember.UserId.ToString()).PromotedToAdmin(promotedMember.ConversationId)
            }
            .Concat(new[]
            {
                // Send the system message to the conversation group
                _hub.Clients.Groups(systemMessage.ConversationId.ToString()).MessageReceived(systemMessage.ToDTO())
            })
            .Concat(
                // Notify all members about the conversation update
                memberIds.Select(userId =>
                    _hub.Clients.User(userId.ToString()).MemberRoleChanged(systemMessage.ConversationId))
            )
            .Concat(memberIds.Select(userId =>
                // Notify all members about the new message
                _hub.Clients.User(userId.ToString()).HasNewMessage(systemMessage.ConversationId)));

            // Await all notifications concurrently
            await Task.WhenAll(allNotificationTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system message notification to conversation {ConversationId}", systemMessage.ConversationId);
        }
    }


    private bool HasAdminPromotePermission(MemberRole memberRole) =>
        memberRole == MemberRole.Owner || memberRole == MemberRole.Admin;
}

public sealed class MakeMemberAdminCommandValidator : AbstractValidator<MakeMemberAdminCommand>
{
    public MakeMemberAdminCommandValidator(ApplicationDbContext dbContext)
    {
        RuleFor(x => x.ConversationMemberId)
            .GreaterThan(0)
            .WithMessage("Invalid conversation member id.");

        RuleFor(x => x.ConversationMemberId)
            .MustAsync(async (conversationMemberId, cancellationToken) =>
            {
                return await dbContext.ConversationMembers
                    .AnyAsync(x => x.MemberId == conversationMemberId && x.IsActive && x.LeftAt == null, cancellationToken);
            })
            .WithMessage("The member is not active in this conversation or has already left.");

        // Rule: Member should not already be an admin or owner
        RuleFor(x => x.ConversationMemberId)
            .MustAsync(async (conversationMemberId, cancellationToken) =>
            {
                var member = await dbContext.ConversationMembers
                    .FirstOrDefaultAsync(x => x.MemberId == conversationMemberId, cancellationToken);

                if (member == null) return false; // Already handled above

                return member.Role == MemberRole.Member; // Only regular members can be promoted
            })
            .WithMessage("Member is already an admin or owner.");
    }
}