namespace SyncChat.API.Shared.ResultHandling;

public static class CustomResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess) throw new InvalidOperationException();

        return Results.Problem(
            title: GetTitle(result.Error),
            detail: GetDetail(result.Error),
            type: GetType(result.Error.Type),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: GetErrors(result));

        static string GetTitle(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Code,
                ErrorType.Problem => error.Code,
                ErrorType.NotFound => error.Code,
                ErrorType.Conflict => error.Code,
                ErrorType.Forbidden => error.Code,
                ErrorType.Unauthorized => error.Code,
                _ => "Server failure",
            };

        static string GetDetail(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Description,
                ErrorType.Problem => error.Description,
                ErrorType.NotFound => error.Description,
                ErrorType.Conflict => error.Description,
                ErrorType.Forbidden => error.Description,
                ErrorType.Unauthorized => error.Description,
                _ => "An unexpected error occurred.",
            };

        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.2",
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                ErrorType.Forbidden => "https://tools.ietf.org/doc/html/rfc7231#section-6.5.3",
                ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                _ => "https://tools.ietf.org/html/rfc7231#section-6",
            };

        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Problem => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError,
            };

        static Dictionary<string, object?>? GetErrors(Result result)
        {
            if (result.Error is not ValidationError validationError) return null;

            return new Dictionary<string, object?>
            {
                { "errors", validationError.Errors.Select(x => x.Description) }
            };
        }
    }
}
