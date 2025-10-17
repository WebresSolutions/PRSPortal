using System.Net;

namespace Portal.Shared.ResponseModels;

/// <summary>
/// Generic result wrapper for API responses
/// Provides consistent error handling and success/failure indication
/// </summary>
/// <typeparam name="T">The type of data contained in the successful result</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets or sets the generic nullable value containing the successful result data
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Gets or sets the optional error type if the operation failed
    /// </summary>
    public ErrorType? Error { get; set; }

    /// <summary>
    /// Gets a value indicating whether the result represents a successful operation
    /// Returns true when there is no error and a value is present
    /// </summary>
    public bool IsSuccess => (Error is null || Error is ErrorType.None) && Value is not null;

    /// <summary>
    /// Gets or sets the human-readable description of the error if one occurred
    /// </summary>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Sets the error information for the result object
    /// </summary>
    /// <param name="errorType">The type of error that occurred</param>
    /// <param name="description">The human-readable description of the error</param>
    public void SetError(ErrorType errorType, string? description = null)
    {
        Error = errorType;
        ErrorDescription = description;
    }

    /// <summary>
    /// Converts HTTP status codes to appropriate error types for the result
    /// Maps common HTTP status codes to internal error types
    /// </summary>
    /// <param name="code">The HTTP status code to convert</param>
    public void ConvertHttpResponseToError(HttpStatusCode code)
    {
        Error = code switch
        {
            HttpStatusCode.BadRequest => ErrorType.BadRequest,
            HttpStatusCode.Forbidden => ErrorType.BadRequest,
            HttpStatusCode.NotFound => ErrorType.BadRequest,
            HttpStatusCode.InternalServerError => ErrorType.InternalError,
            HttpStatusCode.NotImplemented => ErrorType.InternalError,
            HttpStatusCode.Unauthorized => ErrorType.Unauthorized,
            HttpStatusCode.OK => null,
            _ => null
        };
    }
}