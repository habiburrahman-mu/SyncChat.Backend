using FluentValidation;
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
    string InitialMessge) : ICommand<long>;

public sealed class CreateConversationCommandHandler : ICommandHandler<CreateConversationCommand, long>
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

public sealed class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.InitialMessge)
            .NotEmpty().WithMessage($"{nameof(CreateConversationCommand.InitialMessge)} cannot be empty.");

        RuleFor(x => x.MemberIdList)
            .NotEmpty().WithMessage($"{nameof(CreateConversationCommand.MemberIdList)} cannot be empty.")
            .Must(memberIds => memberIds.Distinct().Count() == memberIds.Count)
                .WithMessage($"{nameof(CreateConversationCommand.MemberIdList)} must not contain duplicate IDs.")
            .Must(memberIds => memberIds.All(id => id > 0))
                .WithMessage($"{nameof(CreateConversationCommand.MemberIdList)} must contain valid user IDs greater than zero.")
            .Must((x, memberIds) => memberIds.Any(id => id == x.CreatedBy))
                .WithMessage($"{nameof(CreateConversationCommand.CreatedBy)} must be included in {nameof(CreateConversationCommand.MemberIdList)}.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage($"{nameof(CreateConversationCommand.Type)} must be a valid {nameof(ConversationType)} enum value.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage($"{nameof(CreateConversationCommand.Name)} cannot be empty.")
            .When(x => x.Type == ConversationType.Group);

        RuleFor(x => x.CreatedBy)
            .GreaterThan(0).WithMessage($"{nameof(CreateConversationCommand.CreatedBy)} must be a valid user ID greater than zero.");
    }
}