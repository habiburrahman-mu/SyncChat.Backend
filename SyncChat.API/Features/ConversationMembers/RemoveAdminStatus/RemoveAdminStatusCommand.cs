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

namespace SyncChat.API.Features.ConversationMembers.RemoveAdminStatus;

public sealed record RemoveAdminStatusCommand(long ConversationMemberId) : ICommand;

public sealed class RemoveAdminStatusCommandHandler : ICommandHandler<RemoveAdminStatusCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IIdentityService _identity;
    private readonly IMessageNotificationService _notificationService;

    public RemoveAdminStatusCommandHandler(
        ApplicationDbContext dbContext,
        IIdentityService identity,
        IMessageNotificationService notificationService)
    {
        _dbContext = dbContext;
        _identity = identity;
        _notificationService = notificationService;
    }

    public async Task<Result> HandleAsync(RemoveAdminStatusCommand command, CancellationToken cancellationToken)
    {
        var member = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(m => m.MemberId == command.ConversationMemberId, cancellationToken);

        if (member is null)
            return Result.Failure(ConversationMemberErrors.NotFound(command.ConversationMemberId));

        long actorId = _identity.GetUserID();

        var actor = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(m => m.UserId == actorId &&
                                      m.ConversationId == member.ConversationId,
                                      cancellationToken);

        if (actor is null || (actor.Role != MemberRole.Owner && actor.Role != MemberRole.Admin))
            return Result.Failure(ConversationMemberErrors.Forbidden());

        // Already not an admin (idempotent)
        if (member.Role != MemberRole.Admin)
            return Result.Success();

        member.Role = MemberRole.Member;
        _dbContext.ConversationMembers.Update(member);

        var systemMessage = await BuildSystemMessage(member, actorId, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyMemberDemotedAsync(
            new MemberDemotedNotificationModel(
                member.ConversationId,
                member.UserId,
                systemMessage.ToDTO()),
            cancellationToken);

        return Result.Success();
    }

    private async Task<Message> BuildSystemMessage(
        ConversationMember member, long actorId, CancellationToken ct)
    {
        var msg = new Message
        {
            Uuid = Guid.NewGuid(),
            ConversationId = member.ConversationId,
            SenderId = actorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Type = MessageType.System,
            MetaData = SystemMessageHelper.AdminStatusRemoved(member.UserId, actorId)
        };

        await _dbContext.Messages.AddAsync(msg, ct);

        var conv = await _dbContext.Conversations
            .FirstAsync(c => c.ConversationId == member.ConversationId, ct);

        conv.LastMessage = msg;
        _dbContext.Conversations.Update(conv);

        return msg;
    }

    public sealed class RemoveAdminStatusCommandValidator : AbstractValidator<RemoveAdminStatusCommand>
    {
        public RemoveAdminStatusCommandValidator(ApplicationDbContext dbContext)
        {
            RuleFor(x => x.ConversationMemberId)
                .GreaterThan(0);

            RuleFor(x => x.ConversationMemberId)
                .MustAsync(async (id, ct) =>
                    await dbContext.ConversationMembers.AnyAsync(m =>
                        m.MemberId == id &&
                        m.IsActive &&
                        m.LeftAt == null, ct)
                )
                .WithMessage("Member is not active in this conversation.")
                .WithName(nameof(RemoveAdminStatusCommand.ConversationMemberId));

            RuleFor(x => x.ConversationMemberId)
                .MustAsync(async (id, ct) =>
                {
                    var member = await dbContext.ConversationMembers
                        .FirstOrDefaultAsync(m => m.MemberId == id, ct);

                    return member != null && member.Role != MemberRole.Owner;
                })
                .WithMessage("Cannot change owner role.")
                .WithName(nameof(RemoveAdminStatusCommand.ConversationMemberId));

            RuleFor(x => x.ConversationMemberId)
                .MustAsync(async (id, ct) =>
                {
                    var member = await dbContext.ConversationMembers
                        .FirstOrDefaultAsync(m => m.MemberId == id, ct);

                    if (member == null || member.Role != MemberRole.Admin)
                        return false;

                    int adminCount = await dbContext.ConversationMembers.CountAsync(m =>
                        m.ConversationId == member.ConversationId &&
                        m.Role == MemberRole.Admin &&
                        m.IsActive &&
                        m.LeftAt == null, ct);

                    return adminCount > 1;
                })
                .WithMessage("Cannot remove admin role because this is the only active admin.")
                .WithName(nameof(RemoveAdminStatusCommand.ConversationMemberId));
        }
    }
}
