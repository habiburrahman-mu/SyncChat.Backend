using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Conversations.CreateConversation;

public sealed record CreateConversationCommand(
    long CreatedBy,
    List<long> MemberIdList,
    string? Name,
    ConversationType Type,
    string InitialMessge) : ICommand<Result<long>>;

public sealed class CreateConversationCommandHandler : ICommandHandler<CreateConversationCommand, Result<long>>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateConversationCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<long>> HandleAsync(CreateConversationCommand command, CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        Conversation conversation = new()
        {
            Uuid = Guid.NewGuid(),
            Type = command.Type,
            Name = command.Name,
            CreatedBy = command.CreatedBy,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Settings = "{}",
            IsDeleted = false,
        };

        await _dbContext.Conversations.AddAsync(conversation, cancellationToken);

        Message message = new()
        {
            Uuid = Guid.NewGuid(),
            Conversation = conversation,
            SenderId = command.CreatedBy,
            Type = MessageType.Text,
            Content = command.InitialMessge,
            MetaData = "{}",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsEdited = false,
            EditedAt = null,
        };

        await _dbContext.Messages.AddAsync(message, cancellationToken);

        List<ConversationMember> members = command.MemberIdList
            .Select(memberId => new ConversationMember
            {
                Conversation = conversation,
                UserId = memberId,
                Role = memberId == command.CreatedBy ? MemberRole.Admin : MemberRole.Member,
                JoinedAt = DateTimeOffset.UtcNow,
                Settings = "{}",
                IsActive = true
            })
            .ToList();

        await _dbContext.ConversationMembers.AddRangeAsync(members, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        conversation.LastMessageId = message.MessageId;

        _dbContext.Conversations.Update(conversation);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbContext.Database.CommitTransactionAsync(cancellationToken);

        return conversation.ConversationId;
    }
}