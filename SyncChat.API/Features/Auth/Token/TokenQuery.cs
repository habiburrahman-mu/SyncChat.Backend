using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.Token;

public sealed record TokenQuery(string UserName, string Password) : IQuery<Result<string>>;