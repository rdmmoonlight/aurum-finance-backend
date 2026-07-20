namespace Aurum.Api.Infrastructure.Security;

/// <summary>
/// Reads the acting user out of the current HTTP request's validated JWT
/// claims. Every feature resolves "who is making this call" through this
/// single service — mirrors how the previous NestJS backend's
/// common/current-user.ts::getCurrentUserId() was the one place every
/// service read the authenticated user from.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The authenticated user's id. Throws UnauthorizedAppException if
    /// there is no authenticated user on the current request — this should
    /// only happen if a service is called from a code path that isn't
    /// behind [Authorize], which is itself a bug to fix, not a case to
    /// silently tolerate.
    /// </summary>
    Guid UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }
}
