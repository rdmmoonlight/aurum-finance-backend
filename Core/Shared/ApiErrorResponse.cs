namespace Aurum.Api.Core.Shared;

/// <summary>
/// Standard envelope for every error response returned by the API.
/// </summary>
public sealed class ApiErrorResponse
{
    public bool Success { get; init; } = false;

    public required string ErrorCode { get; init; }

    public required string Message { get; init; }

    /// <summary>
    /// Optional per-field validation errors (present only for HTTP 400
    /// responses raised from FluentValidation or manual validation checks).
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    /// <summary>
    /// Correlation identifier for the request, useful for cross-referencing
    /// Serilog log entries when investigating an issue.
    /// </summary>
    public required string TraceId { get; init; }
}
