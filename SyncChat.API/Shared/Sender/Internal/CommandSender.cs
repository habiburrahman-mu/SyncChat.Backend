using FluentValidation;
using FluentValidation.Results;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Shared.Sender.Internal;

public class CommandSender(IServiceProvider serviceProvider) : ICommandSender
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {

        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);

        if (handler is null)
        {
            throw new InvalidOperationException($"No handler found for command type {command.GetType().Name}");
        }

        Result validationResult = await ValidateCommandAsync(command, cancellationToken);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        var result = await handler.HandleAsync((dynamic)command, cancellationToken) as Result;
        return result!;
    }

    public async Task<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);

        if (handler is null)
        {
            throw new InvalidOperationException($"No handler found for command type {command.GetType().Name}");
        }

        Result validationResult = await ValidateCommandAsync(command, cancellationToken);

        if(validationResult.IsFailure)
        {
            return Result.Failure<TResponse>(validationResult.Error);
        }

        var result = await handler.HandleAsync((dynamic)command, cancellationToken) as Result<TResponse>;

        return result!;
    }

    private async Task<Result> ValidateCommandAsync(ICommand command, CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(command.GetType());
        var validator = _serviceProvider.GetService(validatorType);

        if (validator is not null)
        {
            dynamic dynamicValidator = validator;
            var result = await dynamicValidator.ValidateAsync((dynamic)command, cancellationToken);

            if (!result.IsValid)
            {
                var errors = ((IEnumerable<object>)result.Errors).Cast<ValidationFailure>()
                    .Select(f => Error.Validation(f.ErrorCode ?? "Validation.Error", f.ErrorMessage))
                    .ToArray();

                return Result.Failure(new ValidationError(errors));
            }
        }

        return Result.Success();
    }
}

