using System.Text.Json;
using Aurum.Api.Core.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Aurum.Api.Infrastructure.Security;

/// <summary>
/// Without this, a failed [Authorize(Roles = "...")] check produces an
/// empty HTTP 403 body. This routes that case through the same
/// ErrorResponse {error} shape as every other error in the API (see
/// AuthenticationServiceExtensions' OnChallenge for the equivalent 401
/// case). Falls back to the framework's default handler for every other
/// outcome (success, unauthenticated challenge).
/// </summary>
public sealed class AppAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden && context.User.Identity?.IsAuthenticated == true)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new ErrorResponse
            {
                Error = "You do not have permission to perform this action.",
            });

            await context.Response.WriteAsync(body);
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
