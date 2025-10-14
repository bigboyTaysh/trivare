using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Settings;

namespace Trivare.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
        {
            Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
            Subject = "Password Reset Request",
            Body = $"Please reset your password by using the following token: {resetToken}",
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}
