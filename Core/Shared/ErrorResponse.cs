namespace Aurum.Api.Core.Shared;

/// <summary>
/// The wire shape for every error response, matching the previous NestJS
/// backend's global exception filter exactly: <c>{ "error": "message" }</c>.
/// Kept intentionally minimal (no errorCode/traceId/success fields) so the
/// Next.js frontend's existing <c>err?.error</c> call sites keep working
/// unchanged when pointed at this API.
/// </summary>
public sealed class ErrorResponse
{
    public required string Error { get; init; }
}
