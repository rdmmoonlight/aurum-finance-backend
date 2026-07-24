using Aurum.Api.Features.Authentication;
using Aurum.Api.Features.Users.Entities;
using Aurum.Api.Infrastructure.Email;
using Aurum.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

namespace Aurum.Api.Core.Extensions;

public static class SecurityServiceExtensions
{
    /// <summary>
    /// Registers the building blocks the Authentication feature (and every
    /// later feature that needs to know "who is calling") depends on:
    /// password hashing, JWT issuing, refresh-token rotation, current-user
    /// resolution, and email delivery for verification/reset links.
    /// </summary>
    public static IServiceCollection AddAppSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();

        // 1. Bind konfigurasi Smtp dari appsettings.json
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        // 2. Ganti LoggingEmailSender dengan SmtpEmailSender
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}