using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    long UserID,
    JsonPatchDocument<UpdateUserRequest> PatchDocument): ICommand<UpdateUserResponse>;

public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly ApplicationDbContext dbContext;

    public UpdateUserCommandHandler(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Result<UpdateUserResponse>> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.UserID == command.UserID);

        if (user == null)
            return Result.Failure<UpdateUserResponse>(UserErrors.NotFound(command.UserID));

        UpdateUserRequest userDto = new UpdateUserRequest(
            Name: user.Name,
            Email: user.Email,
            Phone: user.Phone);

        command.PatchDocument.ApplyTo(userDto);

        user.Name = userDto.Name;
        user.Email = userDto.Email;
        user.Phone = userDto.Phone;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateUserResponse(
            UserID: user.UserID,
            UUID: user.UUID,
            UserName: user.UserName,
            Name: user.Name,
            Email: user.Email,
            Phone: user.Phone,
            Profile: user.Profile,
            Status: user.Status,
            LastActive: user.LastActive,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            IsVerified: user.IsVerified,
            IsBanned: user.IsBanned);

    }
}