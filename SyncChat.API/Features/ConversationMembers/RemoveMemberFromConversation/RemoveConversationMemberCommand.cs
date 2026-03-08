using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.Notification.Contracts;
using SyncChat.API.Shared.Notification.Contracts.Models;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Utilities;

namespace SyncChat.API.Features.ConversationMembers.RemoveMemberFromConversation;

public sealed record RemoveConversationMemberCommand(
    long ConverstionMemberId) : ICommand;


public sealed class RemoveConversationMemberCommandHandler : ICommandHandler<RemoveConversationMemberCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IMessageNotificationService _notificationService;

    public RemoveConversationMemberCommandHandler(
        ApplicationDbContext dbContext,
        IIdentityService identityService,
        IMessageNotificationService notificationService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _notificationService = notificationService;
    }

    public async Task<Result> HandleAsync(RemoveConversationMemberCommand command, CancellationToken cancellationToken = default)
    {
        var conversationMember = await _dbContext.ConversationMembers.FirstOrDefaultAsync(x => x.MemberId == command.ConverstionMemberId, cancellationToken);

        if (conversationMember is null)
            return Result.Failure(ConversationMemberErrors.NotFound(command.ConverstionMemberId));

        // Check if user has necessary permissions
        long actorId = _identityService.GetUserID();

        bool selfLeave = actorId == conversationMember.UserId;

        if (!selfLeave)
        {
            ConversationMember? actorMembership = await _dbContext.ConversationMembers
                .FirstOrDefaultAsync(x => x.UserId == actorId && x.ConversationId == conversationMember.ConversationId, cancellationToken);

            if (actorMembership is null || !HasMemberRemovePermission(actorMembership.Role))
                return Result.Failure(ConversationMemberErrors.Forbidden());
        }

        conversationMember.LeftAt = DateTime.UtcNow;
        conversationMember.IsActive = false;

        _dbContext.ConversationMembers.Update(conversationMember);

        Message systemMessage = await AddSystemMessageAsync(conversationMember, actorId, selfLeave, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyMemberRemovedAsync(
            new MemberRemovedNotificationModel(
                conversationMember.ConversationId,
                conversationMember.UserId,
                systemMessage.ToDTO()),
            cancellationToken);

        return Result.Success();
    }

    private async Task<Message> AddSystemMessageAsync(ConversationMember member, long actorId, bool selfLeave, CancellationToken cancellationToken)
    {
        Message message = new()
        {
            Uuid = Guid.NewGuid(),
            ConversationId = member.ConversationId,
            SenderId = actorId,
            Type = MessageType.System,
            MetaData = selfLeave ? SystemMessageHelper.MemberLeft(member.UserId) : SystemMessageHelper.MemberRemoved(member.UserId, actorId),
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

    private bool HasMemberRemovePermission(MemberRole memberRole) => memberRole == MemberRole.Owner || memberRole == MemberRole.Admin;
}

public sealed class RemoveConversationMemberCommandValidator : AbstractValidator<RemoveConversationMemberCommand>
{
    public RemoveConversationMemberCommandValidator(ApplicationDbContext dbContext)
    {
        RuleFor(x => x.ConverstionMemberId)
            .GreaterThan(0)
            .WithMessage("Invalid conversation member id.");

        RuleFor(x => x.ConverstionMemberId)
            .MustAsync(async (conversationMemberId, cancellationToken) =>
            {
                return await dbContext.ConversationMembers
                    .AnyAsync(x => x.MemberId == conversationMemberId
                                   && x.IsActive
                                   && x.LeftAt == null, cancellationToken);
            })
            .WithMessage("The member is not active in this conversation or has already left.");

        // Rule: if the member is the only owner, cannot remove without assigning new owner
        RuleFor(x => x.ConverstionMemberId)
            .MustAsync(async (conversationMemberId, cancellationToken) =>
            {
                var member = await dbContext.ConversationMembers
                    .FirstOrDefaultAsync(x => x.MemberId == conversationMemberId, cancellationToken);

                if (member == null) return false; // Already handled above

                if (member.Role == MemberRole.Owner)
                {
                    int ownerCount = await dbContext.ConversationMembers
                        .CountAsync(x => x.ConversationId == member.ConversationId
                                         && x.Role == MemberRole.Owner
                                         && x.IsActive
                                         && x.LeftAt == null, cancellationToken);

                    return ownerCount > 1; // Only allow removal if there is another owner
                }

                return true; // Non-owner members are fine
            })
            .WithMessage("Cannot remove the only owner. Please assign a new owner first.");

        // Rule: if the member is the only admin, cannot remove without assigning new admin
        RuleFor(x => x.ConverstionMemberId)
            .MustAsync(async (conversationMemberId, cancellationToken) =>
            {
                var member = await dbContext.ConversationMembers
                    .FirstOrDefaultAsync(x => x.MemberId == conversationMemberId, cancellationToken);

                if (member == null) return false;

                if (member.Role == MemberRole.Admin)
                {
                    int adminCount = await dbContext.ConversationMembers
                        .CountAsync(x => x.ConversationId == member.ConversationId
                                         && x.Role == MemberRole.Admin
                                         && x.IsActive
                                         && x.LeftAt == null, cancellationToken);

                    return adminCount > 1; // Only allow removal if there is another admin
                }

                return true;
            })
            .WithMessage("Cannot remove the only admin. Please assign a new admin first.");
    }
}
