namespace SyncChat.API.Shared.ResultHandling;

public record ValidationError : Error
{
    private readonly Error[] Errors;

    public ValidationError(Error[] errors)
        : base(
            "Validation.General",
            "One or more validation",
            ErrorType.Validation)
    {
        this.Errors = errors;
    }

    public static ValidationError FromResults(IEnumerable<Result> results) => 
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}
