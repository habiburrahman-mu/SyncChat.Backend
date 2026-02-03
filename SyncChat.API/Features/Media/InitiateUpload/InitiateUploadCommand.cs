using FluentValidation;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Features.Media.InitiateUpload;

public sealed record InitiateUploadCommand(
    MediaOwnerDto Owner,
    MediaFileDescriptorDto File) : ICommand<InitiateUploadResponse>;

public sealed class InitiateUploadCommandHandler : ICommandHandler<InitiateUploadCommand, InitiateUploadResponse>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IBlobStorage blobStorage;

    public InitiateUploadCommandHandler(ApplicationDbContext dbContext, IBlobStorage blobStorage)
    {
        this.dbContext = dbContext;
        this.blobStorage = blobStorage;
    }

    public Task<Result<InitiateUploadResponse>> HandleAsync(InitiateUploadCommand request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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