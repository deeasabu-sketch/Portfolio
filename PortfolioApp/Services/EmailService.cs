using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PortfolioApp.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var section = _config.GetSection("Smtp");
        var fromEmail = section["FromEmail"] ?? throw new InvalidOperationException("Smtp:FromEmail is not configured.");
        var appPassword = section["AppPassword"] ?? throw new InvalidOperationException("Smtp:AppPassword is not configured.");
        var host = section["Host"] ?? "smtp.gmail.com";
        var port = int.TryParse(section["Port"], out var p) ? p : 587;
        var fromName = section["FromName"] ?? "MD. Abu Saeed Portfolio";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(fromEmail, appPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
