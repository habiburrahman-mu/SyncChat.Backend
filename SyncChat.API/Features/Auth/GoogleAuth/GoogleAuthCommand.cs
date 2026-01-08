using FluentValidation;
using SyncChat.API.Infrastructure.AuthProviders.Google;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.GoogleAuth;

public sealed record GoogleAuthCommand(string IdToken, string DeviceIdentifier) : ICommand<GoogleAuthResponse>;

public sealed class GoogleAuthCommandHandler : ICommandHandler<GoogleAuthCommand, GoogleAuthResponse>
{
    private readonly IGoogleTokenValidator googleTokenValidator;

    public GoogleAuthCommandHandler(IGoogleTokenValidator googleTokenValidator)
    {
        this.googleTokenValidator = googleTokenValidator;
    }

    public async Task<Result<GoogleAuthResponse>> HandleAsync(GoogleAuthCommand command, CancellationToken cancellationToken)
    {
        // TODO: Validate the Google ID token and authenticate the user.

        var googleUserInfo = await googleTokenValidator.ValidateAsync(command.IdToken);


        throw new NotImplementedException();
    }
}

public sealed class GoogleAuthCommandValidator : AbstractValidator<GoogleAuthCommand>
{
    public GoogleAuthCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("ID token must not be empty.");

        RuleFor(x => x.DeviceIdentifier)
            .NotEmpty()
            .WithMessage("Device identifier must not be empty.");
    }
}