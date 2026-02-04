
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.MediaManagement.ConfirmUpload;

public sealed record ConfirmUploadRequest(Guid MediaId);

public sealed class ConfirmUploadEndpoint : IMediaEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(MediaRoute.ConfirmUpload,
            async ([FromBody] ConfirmUploadRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
            {
                ConfirmMediaUploadCommand command = new(MediaId: request.MediaId);

                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(result => Results.Ok(result), CustomResults.Problem);
            })
            .WithSummary("Confirm Media Upload")
            .Produces<ConfirmMediaUploadResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem();
    }
}
