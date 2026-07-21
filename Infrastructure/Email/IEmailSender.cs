namespace Aurum.Api.Infrastructure.Email;

/// <summary>
/// Abstraction over "send this email". Every caller (AuthService) depends
/// only on this interface, so the concrete sender can be swapped later —
/// e.g. for a real SMTP relay or transactional-email API — without
/// touching any business logic. See LoggingEmailSender for the default,
/// zero-cost implementation registered today.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}
