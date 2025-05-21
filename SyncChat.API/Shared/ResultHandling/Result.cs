namespace SyncChat.API.Shared.ResultHandling;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error ", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess ? _value!
        : throw new InvalidOperationException("Result is not successful, no value available.");

    public static Result<T> ValidationFailure(Error error) =>
        new(default, false, error);

    public static implicit operator Result<T>(T? value) =>
        value is not null ? Success(value) : Failure<T>(Error.NullValue);

}