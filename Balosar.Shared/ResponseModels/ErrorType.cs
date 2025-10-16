namespace Portal.Shared.ResponseModels;

/// <summary>
/// Enumeration of error types that can occur in API responses
/// Used for categorizing and handling different types of errors consistently
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// No error occurred (default state)
    /// </summary>
    None,             // Optional default

    /// <summary>
    /// The requested resource was not found
    /// </summary>
    NotFound,

    /// <summary>
    /// The request data failed validation
    /// </summary>
    BadRequest,

    /// <summary>
    /// The user is not authenticated
    /// </summary>
    Unauthorized,

    /// <summary>
    /// The user is not authorized to access the requested resource
    /// </summary>
    Forbidden,

    /// <summary>
    /// The request conflicts with the current state of the resource
    /// </summary>
    Conflict,

    /// <summary>
    /// An internal server error occurred
    /// </summary>
    InternalError,

    /// <summary>
    /// The request timed out
    /// </summary>
    Timeout,

    /// <summary>
    /// A dependency service failed
    /// </summary>
    DependencyFailure,

    /// <summary>
    /// An unknown or unexpected error occurred
    /// </summary>
    Unknown
}