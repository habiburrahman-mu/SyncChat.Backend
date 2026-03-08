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

namespace SyncChat.API.Features.ConversationMembers.MakeMemberAdmin;

public sealed record MakeMemberAdminCommand(
    long ConversationMemberId) : ICommand;

public sealed class MakeMemberAdminCommandHandler : ICommandHandler<MakeMemberAdminCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IMessageNotificationService _notificationService;

    public MakeMemberAdminCommandHandler(
        ApplicationDbContext dbContext,
        IIdentityService identityService,
        IMessageNotificationService notificationService)
    {
        this._dbContext = dbContext;
        this._identityService = identityService;
        this._notificationService = notificationService;
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

        await _notificationService.NotifyMemberPromotedAsync(
            new MemberPromotedNotificationModel(
                conversationMember.ConversationId,
                conversationMember.UserId,
                systemMessage.ToDTO()),
            cancellationToken);

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