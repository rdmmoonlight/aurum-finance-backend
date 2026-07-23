namespace Aurum.Api.Infrastructure.Email;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = string.Empty; // <-- Pastikan 'N' kapital (UserName)
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Aurum Finance";
    public bool EnableSsl { get; set; } = true;         // <-- Pastikan bernama EnableSsl
}