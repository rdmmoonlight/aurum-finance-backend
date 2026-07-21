using System.Text;
using System.Text.Json;
using Aurum.Api.Core.Shared;
using Aurum.Api.Infrastructure.Security; // <-- Mengimpor AuthSettings & AppAuthorizationMiddlewareResultHandler
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Aurum.Api.Core.Extensions;

public static class AuthenticationServiceExtensions
{
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
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, AppAuthorizationMiddlewareResultHandler>();

        // Sekarang AuthSettings sudah terbaca dengan baik
        services.Configure<AuthSettings>(configuration.GetSection(AuthSettings.SectionName));

        return services;
    }
}