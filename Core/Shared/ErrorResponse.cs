namespace Aurum.Api.Core.Shared;

/// <summary>
/// The wire shape for every error response: <c>{ "error": "message" }</c>.
/// Kept intentionally minimal (no errorCode/traceId/success fields).
/// </summary>
public sealed class ErrorResponse
{
    public required string Error { get; init; }
}
