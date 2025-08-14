using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Features.Notifications;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Conversations.CreateConversation;

public sealed record CreateConversationCommand(
    long CreatedBy,
    List<long> MemberIdList,
    string? Name,
    ConversationType Type) : ICommand<long>;

public sealed class CreateConversationCommandHandler : ICommandHandler<CreateConversationCommand, long>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<NotificationHub, INotificationClient> _hub;
    private readonly IIdentityService _identityService;

    public CreateConversationCommandHandler(ApplicationDbContext dbContext, IHubContext<NotificationHub, INotificationClient> hub, IIdentityService identityService)
    {
        _dbContext = dbContext;
        _hub = hub;
        _identityService = identityService;
    }

    public async Task<Result<long>> HandleAsync(CreateConversationCommand command, CancellationToken cancellationToken = default)
    {
        //await _dbContext.Database.BeginTransactionAsync(cancellationToken);

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

        //Message message = new()
        //{
        //    Uuid = Guid.NewGuid(),
        //    Conversation = conversation,
        //    SenderId = command.CreatedBy,
        //    Type = MessageType.Text,
        //    Content = command.InitialMessge,
        //    MetaData = "{}",
        //    CreatedAt = DateTimeOffset.UtcNow,
        //    UpdatedAt = DateTimeOffset.UtcNow,
        //    IsEdited = false,
        //    EditedAt = null,
        //};

        //await _dbContext.Messages.AddAsync(message, cancellationToken);

        List<ConversationMember> members = command.MemberIdList
            .Select(memberId => new ConversationMember
            {
                Conversation = conversation,
                UserId = memberId,
                Role = memberId == command.CreatedBy ? MemberRole.Owner : MemberRole.Member,
                JoinedAt = DateTimeOffset.UtcNow,
                Settings = "{}",
                IsActive = true
            })
            .ToList();

        await _dbContext.ConversationMembers.AddRangeAsync(members, cancellationToken);

        //await _dbContext.SaveChangesAsync(cancellationToken);

        //conversation.LastMessageId = message.MessageId;

        //_dbContext.Conversations.Update(conversation);

        //await _dbContext.SaveChangesAsync(cancellationToken);

        //await _dbContext.Database.CommitTransactionAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await SendNotificationAsync(command, conversation, cancellationToken);

        return conversation.ConversationId;
    }

    private async Task SendNotificationAsync(CreateConversationCommand command, Conversation conversation, CancellationToken cancellationToken)
    {
        List<long> memberList = command.MemberIdList
                                .Where(memberId => memberId != _identityService.GetUserID())
                                .ToList();

        ConversationDTO conversationDTO = conversation.ToDTO()!;
        conversationDTO.LastMessage = command.InitialMessge;
        conversationDTO.OtherUserId = conversation.Type == ConversationType.Direct ? memberList.First() : null;

        var notificationTasks = memberList
            .Select(userId => this._hub.Clients.User(userId.ToString()).NewConversationCreated(conversationDTO));

        await Task.WhenAll(notificationTasks);
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