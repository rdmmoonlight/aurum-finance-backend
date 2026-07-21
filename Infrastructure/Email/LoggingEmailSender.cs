using Microsoft.Extensions.Logging;

namespace Aurum.Api.Infrastructure.Email;

/// <summary>
/// Default IEmailSender: writes the message to the application log instead
/// of sending it through a paid provider. Deliberately the out-of-the-box
/// choice — it costs nothing to run and needs no external account, so
/// registration, password reset, and email verification all work in
/// development (and in production, until a real provider is configured)
/// without any billing dependency.
///
/// To send real email later, implement IEmailSender against whichever
/// provider is chosen and replace the registration in
/// SecurityServiceExtensions.AddAppSecurityServices — no other code needs
/// to change.
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Email suppressed — no provider configured. To: {ToEmail}, Subject: {Subject}, Body: {Body}",
            toEmail,
            subject,
            htmlBody);

        return Task.CompletedTask;
    }
}
