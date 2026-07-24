using Aurum.Api.Features.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Aurum.Api.Infrastructure.Email;

/// <summary>
/// Bridges ASP.NET Core Identity's built-in account-management endpoints
/// (MapIdentityApi&lt;ApplicationUser&gt;) to the application's own
/// IEmailSender. Identity calls this class directly whenever a user
/// registers, requests a password reset, or resends a confirmation
/// email — there is no custom Authentication feature left in the codebase
/// that triggers these emails.
/// </summary>
public sealed class IdentityEmailSender : IEmailSender<ApplicationUser>
{
    private readonly IEmailSender _emailSender;

    public IdentityEmailSender(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        _emailSender.SendAsync(
            email,
            "Confirm your Aurum email address",
            $"<p>Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.</p>");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        _emailSender.SendAsync(
            email,
            "Reset your Aurum password",
            $"<p>Please reset your password by <a href='{resetLink}'>clicking here</a>.</p>");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        _emailSender.SendAsync(
            email,
            "Reset your Aurum password",
            $"<p>Your password reset code is: <strong>{resetCode}</strong></p>");
}
