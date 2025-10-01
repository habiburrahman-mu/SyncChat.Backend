using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Features.Notifications;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Constants;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.ConversationMembers.AddConversationMember;

public sealed record AddConversationMemberCommand(
    long ConversationId,
    List<long> MemberIds) : ICommand;

public sealed class AddConversationMemberCommandHandler(ApplicationDbContext dbContext, IIdentityService identityService, IHubContext<NotificationHub, INotificationClient> hub)
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

        List<Message> systemMessages = await AddSystemMessages(command, userId, cancellationToken);

        var conversation = await dbContext.Conversations
            .FirstAsync(c => c.ConversationId == command.ConversationId, cancellationToken);

        conversation.LastMessage = systemMessages.LastOrDefault();

        dbContext.Conversations.Update(conversation);

        await dbContext.SaveChangesAsync(cancellationToken);

        await SendNotificationAsync(
            command.ConversationId,
            existingMembers.Select(x => x.UserId).ToList(),
            command.MemberIds,
            systemMessages,
            cancellationToken);

        return Result.Success();
    }

    private async Task<List<Message>> AddSystemMessages(AddConversationMemberCommand command, long userId, CancellationToken cancellationToken)
    {
        var systemMessages = new List<Message>();

        command.MemberIds.ForEach(id =>
        {
            var systemMessage = new Message
            {
                Uuid = Guid.NewGuid(),
                ConversationId = command.ConversationId,
                SenderId = userId,
                Type = MessageType.System,
                Content = null,
                MetaData = JsonConvert.SerializeObject(new
                {
                    Type = SystemMessageType.MemberAdded,
                    UserId = id,
                    AddedBy = userId,
                }),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            systemMessages.Add(systemMessage);
        });

        await dbContext.Messages.AddRangeAsync(systemMessages, cancellationToken);

        return systemMessages;
    }

    private async Task SendNotificationAsync(long conversationId, List<long> existingMembers, List<long> newMembers, List<Message> systemMessages, CancellationToken cancellationToken)
    {
        var conversation = await dbContext.Conversations
            .AsNoTracking()
            .FirstAsync(c => c.ConversationId == conversationId, cancellationToken);

        ConversationDTO conversationDTO = conversation.ToDTO()!;

        var allNotificationTasks = newMembers
            .Select(userId => hub.Clients.User(userId.ToString()).AddedToConversation(conversationDTO))
            .Concat(existingMembers.Select(userId => hub.Clients.User(userId.ToString()).HasNewMessage(conversationId)))
            .Concat(systemMessages.Select(message =>
                hub.Clients.Groups(message.ConversationId.ToString()).MessageReceived(message.ToDTO())))
            .Concat(existingMembers.Select(userId => hub.Clients.User(userId.ToString()).NewMemberAdded(conversationId)));

        await Task.WhenAll(allNotificationTasks);
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

        RuleFor(x => x.ConversationId)
            .MustAsync(async (conversationId, cancellationToken) =>
            {
                return await dbContext.Conversations.AnyAsync(c => c.ConversationId == conversationId && c.Type != ConversationType.Direct, cancellationToken);
            })
            .WithMessage("Cannot add members to a direct conversation.");

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