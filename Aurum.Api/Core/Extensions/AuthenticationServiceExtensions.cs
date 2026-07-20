using System.Text;
using System.Text.Json;
using Aurum.Api.Core.Shared;
using Aurum.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Aurum.Api.Core.Extensions;

public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Registers JWT bearer authentication. The signing key is resolved, in
    /// order of priority, from:
    /// 1. The JWT_SIGNING_KEY environment variable (flat, same convention as
    ///    DATABASE_URL — kept out of the Jwt__* nested env-var form since
    ///    it's a secret most secret managers store as a single flat value).
    /// 2. Jwt:SigningKey in configuration.
    /// Issuer/Audience/AccessTokenExpiryMinutes use the standard ASP.NET
    /// Core Jwt__Issuer / Jwt__Audience / Jwt__AccessTokenExpiryMinutes
    /// environment-variable override convention (see RateLimiting for the
    /// same pattern already in use).
    /// </summary>
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var signingKey =
            Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")
            ?? configuration["Jwt:SigningKey"];

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "No JWT signing key configured. Set the JWT_SIGNING_KEY environment variable " +
                "or Jwt:SigningKey in configuration.");
        }

        services.Configure<JwtSettings>(options =>
        {
            configuration.GetSection(JwtSettings.SectionName).Bind(options);
            options.SigningKey = signingKey;
        });

        var issuer = configuration["Jwt:Issuer"] ?? "Aurum.Api";
        var audience = configuration["Jwt:Audience"] ?? "Aurum.Client";

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Keep claim types exactly as issued ("sub", "email") rather
                // than letting the handler remap them to the long
                // ClaimTypes.* URIs — CurrentUserService reads the raw
                // "sub"/"email" claim names issued by JwtTokenService.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                // Route auth failures through the same ErrorResponse {error} shape
                // as every other error in the API, instead of ASP.NET's
                // default empty 401/challenge response.
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var body = JsonSerializer.Serialize(new ErrorResponse
                        {
                            Error = "Authentication is required or the token is invalid.",
                        });

                        return context.Response.WriteAsync(body);
                    },
                };
            });

        services.AddAuthorization();

        return services;
    }
}
