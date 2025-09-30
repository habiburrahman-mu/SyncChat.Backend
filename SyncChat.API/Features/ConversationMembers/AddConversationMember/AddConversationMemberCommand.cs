using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.ConversationMembers.AddConversationMember;

public sealed record AddConversationMemberCommand(
    long ConversationId,
    List<long> MemberIds) : ICommand;

public sealed class AddConversationMemberCommandHandler(ApplicationDbContext dbContext, IIdentityService identityService)
    : ICommandHandler<AddConversationMemberCommand>
{
    public async Task<Result> HandleAsync(AddConversationMemberCommand command, CancellationToken cancellationToken = default)
    {
        var userId = identityService.GetUserID();

        var existingMembers = await dbContext.ConversationMembers
            .Where(cm => cm.ConversationId == command.ConversationId)
            .ToListAsync(cancellationToken);

        if (!existingMembers.Any(em => em.UserId == userId && (em.Role == MemberRole.Owner || em.Role == MemberRole.Admin)))
        {
            return Result.Failure(ConversationErrors.NotAuthorized(command.ConversationId));
        }

        var newMembers = command.MemberIds
            .Where(id => !existingMembers.Any(em => em.UserId == id))
            .Select(id => new ConversationMember
            {
                ConversationId = command.ConversationId,
                UserId = id,
                Role = MemberRole.Member,
                JoinedAt = DateTimeOffset.UtcNow,
                Settings = "{}",
                IsActive = true
            })
            .ToList();

        await dbContext.ConversationMembers.AddRangeAsync(newMembers, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class AddConversationMemberCommandValidator : AbstractValidator<AddConversationMemberCommand>
{
    public AddConversationMemberCommandValidator(ApplicationDbContext dbContext)
    {
        RuleFor(x => x.ConversationId)
            .GreaterThan(0).WithMessage("ConversationId must be greater than 0.");

        RuleFor(x => x.MemberIds)
            .NotEmpty().WithMessage("MemberIds cannot be empty.")
            .Must(memberIds => memberIds.Distinct().Count() == memberIds.Count)
                .WithMessage("MemberIds must not contain duplicate IDs.")
            .Must(memberIds => memberIds.All(id => id > 0))
                .WithMessage("MemberIds must contain valid user IDs greater than zero.");

        RuleFor(x => x.ConversationId)
            .MustAsync(async (conversationId, cancellationToken) =>
            {
                return await dbContext.Conversations.AnyAsync(c => c.ConversationId == conversationId, cancellationToken);
            })
            .WithMessage("Conversation does not exist.");

        RuleForEach(x => x.MemberIds)
            .MustAsync(async (memberId, cancellationToken) =>
            {
                return await dbContext.Users.AnyAsync(u => u.UserID == memberId, cancellationToken);
            })
            .WithMessage("One or more MemberIds do not correspond to existing users.");

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var existingMemberIds = await dbContext.ConversationMembers
                    .Where(cm => cm.ConversationId == command.ConversationId)
                    .Select(cm => cm.UserId)
                    .ToListAsync(cancellationToken);
                return !command.MemberIds.Any(id => existingMemberIds.Contains(id));
            })
            .WithMessage("One or more MemberIds are already members of the conversation.");
    }
}