
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
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

                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(result => Results.Ok(result), CustomResults.Problem);
            })
            .WithSummary("Initiate media upload intent")
            .Produces<InitiateUploadResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }
}
