using Aurum.Api.Features.Authentication;
using Aurum.Api.Features.Users.Entities;
using Aurum.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

namespace Aurum.Api.Core.Extensions;

public static class SecurityServiceExtensions
{
    /// <summary>
    /// Registers the building blocks the Authentication feature (and every
    /// later feature that needs to know "who is calling") depends on:
    /// password hashing, JWT issuing, current-user resolution.
    /// </summary>
    public static IServiceCollection AddAppSecurityServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
