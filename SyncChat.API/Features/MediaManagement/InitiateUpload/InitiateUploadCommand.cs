using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Constants;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Features.MediaManagement.InitiateUpload;

public sealed record InitiateUploadCommand(
    MediaOwnerDto Owner,
    MediaFileDescriptorDto File) : ICommand<InitiateUploadResponse>;

public sealed class InitiateUploadCommandHandler : ICommandHandler<InitiateUploadCommand, InitiateUploadResponse>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IBlobStorage blobStorage;
    private readonly IIdentityService identityService;

    public InitiateUploadCommandHandler(ApplicationDbContext dbContext, IBlobStorage blobStorage, IIdentityService identityService)
    {
        this.dbContext = dbContext;
        this.blobStorage = blobStorage;
        this.identityService = identityService;
    }

    public async Task<Result<InitiateUploadResponse>> HandleAsync(InitiateUploadCommand request, CancellationToken cancellationToken = default)
    {
        long currentUserId = identityService.GetUserID();

        User? currentUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserID == currentUserId, cancellationToken);

        if(currentUser is null)
            return Result.Failure<InitiateUploadResponse>(UserErrors.NotFound(currentUserId));

        Guid mediaId = Guid.NewGuid();

        bool isUserAvatar = request.Owner.Type == MediaOwnerType.User;
        string storageKey = isUserAvatar
            ? StorageConstants.AvatarKeys.For(currentUser.UUID)
            : StorageConstants.MediaKeys.For(mediaId);

        Media media = new()
        {
            Id = mediaId,
            UserId = currentUser.UUID,
            OwnerType = request.Owner.Type,
            OwnerId = isUserAvatar ? currentUser.UUID.ToString() : request.Owner.Id,
            MimeType = request.File.MimeType,
            SizeBytes = request.File.SizeBytes,
            StorageKey = storageKey,
            State = MediaState.Initiated,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        MediaUploadSession uploadSession = new()
        {
            Id = Guid.NewGuid(),
            MediaId = mediaId,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1), // TODO: Make configurable
            MaxUploads = 1,
            UploadCount = 0,
            UsedAt = null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await dbContext.Media.AddAsync(media, cancellationToken);
        await dbContext.MediaUploadSessions.AddAsync(uploadSession, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var validFor = uploadSession.ExpiresAt - uploadSession.CreatedAt;

            string url = await blobStorage.GeneratePresignedUploadUrlAsync(
                storageKey,
                validFor,
                cancellationToken);

            InitiateUploadResponse response = new(
                MediaId: mediaId, 
                UploadUri: new Uri(url), 
                Expiration: uploadSession.ExpiresAt);

            return response;
        }
        catch
        {
            dbContext.Media.Remove(media);
            dbContext.MediaUploadSessions.Remove(uploadSession);

            await dbContext.SaveChangesAsync(cancellationToken);

            throw;
        }
    }
}

public sealed class InitiateUploadCommandValidator : AbstractValidator<InitiateUploadCommand>
{
    public InitiateUploadCommandValidator()
    {
        RuleFor(x => x.Owner.Id)
            .NotEmpty().WithMessage("Owner ID must not be empty.");

        RuleFor(x => x.Owner.Type)
            .IsInEnum().WithMessage("Owner Type must be valid.");

        RuleFor(x => x.File.MimeType)
            .NotEmpty()
                .WithMessage("File MimeType must not be empty.")
            .Must(MediaConstraints.IsMimeTypeAllowed)
                .WithMessage(x => MediaErrors.UnsupportedMimeType(x.File.MimeType).Description);

        RuleFor(x => x.File.SizeBytes)
            .NotEmpty().WithMessage("File SizeBytes must not be empty.")
            .GreaterThan(0).WithMessage("File SizeBytes must be greater than zero.")
            .LessThanOrEqualTo(MediaConstraints.MaxImageFileSizeBytes)
                .WithMessage(x => MediaErrors.FileTooLarge(MediaConstraints.MaxImageFileSizeBytes).Description);
    }
}