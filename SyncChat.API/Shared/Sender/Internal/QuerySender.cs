using FluentValidation;
using FluentValidation.Results;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SyncChat.API.Shared.Sender.Internal;

public class QuerySender(IServiceProvider serviceProvider) : IQuerySender
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<Result> SendAsync(IQuery query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<>).MakeGenericType(query.GetType());
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);

        if (handler is null)
        {
            throw new InvalidOperationException($"No handler found for query type {query.GetType().Name}");
        }

        Result validationResult = await ValidateCommandAsync(query, cancellationToken);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        return await handler.HandleAsync((dynamic)query, cancellationToken);
    }

    public async Task<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);

        if (handler is null)
        {
            throw new InvalidOperationException($"No handler found for query type {query.GetType().Name}");
        }

        Result validationResult = await ValidateCommandAsync(query, cancellationToken);

        if (validationResult.IsFailure)
        {
            return Result.Failure<TResponse>(validationResult.Error);
        }

        return await handler.HandleAsync((dynamic)query, cancellationToken);
    }

    private async Task<Result> ValidateCommandAsync(IQuery query, CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(query.GetType());
        var validator = _serviceProvider.GetService(validatorType);

        if (validator is not null)
        {
            dynamic dynamicValidator = validator;
            var result = await dynamicValidator.ValidateAsync((dynamic)query, cancellationToken);

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
