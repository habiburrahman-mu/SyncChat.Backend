
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.MediaManagement.InitiateUpload;

public sealed record InitiateUploadRequest(
    MediaOwnerDto Owner,
    MediaFileDescriptorDto File);

public sealed class InitiateUploadEndpoint : IMediaEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(MediaRoute.InitiateUpload,
            async ([FromBody] InitiateUploadRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
            {
                var command = new InitiateUploadCommand(
                    request.Owner,
                    request.File
                );

                await sender.SendAsync(command, cancellationToken);
            });
    }
}
