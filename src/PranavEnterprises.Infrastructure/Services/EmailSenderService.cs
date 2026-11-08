using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PranavEnterprises.Application.Interfaces;

namespace PranavEnterprises.Infrastructure.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(IConfiguration configuration, ILogger<EmailSenderService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendRegistrationNotificationAsync(
        string adminEmail,
        string newUserName,
        string newUserEmail,
        string requestedRole,
        string? jobTitle,
        string? department,
        string approveUrl)
    {
        var subject = $"[ACTION REQUIRED] New Registration: Approve Role for {newUserName}";

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; color: #1e293b; }}
        .card {{ background-color: #ffffff; border-radius: 12px; border: 1px solid #e2e8f0; max-width: 580px; margin: 0 auto; padding: 32px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }}
        .header {{ border-bottom: 2px solid #2563eb; padding-bottom: 16px; margin-bottom: 24px; }}
        .title {{ font-size: 20px; font-weight: 700; color: #0f172a; margin: 0; }}
        .badge {{ display: inline-block; padding: 4px 12px; border-radius: 9999px; font-size: 13px; font-weight: 600; background-color: #dbeafe; color: #1e40af; }}
        .field {{ margin-bottom: 12px; font-size: 14px; }}
        .label {{ font-weight: 600; color: #64748b; }}
        .value {{ color: #0f172a; }}
        .btn {{ display: inline-block; padding: 12px 24px; background-color: #2563eb; color: #ffffff !important; text-decoration: none; border-radius: 6px; font-weight: 600; margin-top: 20px; }}
        .footer {{ font-size: 12px; color: #94a3b8; margin-top: 32px; border-top: 1px solid #f1f5f9; padding-top: 16px; text-align: center; }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='header'>
            <h2 class='title'>Pranav Enterprise &bull; User Registration Notification</h2>
        </div>
        <p>Hello <strong>Pranav Naik (Super Admin)</strong>,</p>
        <p>A new employee account has registered on the enterprise portal and is awaiting your role verification and security approval.</p>

        <div style='background-color: #f8fafc; border-radius: 8px; padding: 16px; margin: 20px 0;'>
            <div class='field'><span class='label'>Full Name:</span> <span class='value'><strong>{newUserName}</strong></span></div>
            <div class='field'><span class='label'>Email Address:</span> <span class='value'>{newUserEmail}</span></div>
            <div class='field'><span class='label'>Job Title:</span> <span class='value'>{jobTitle ?? "Staff"}</span></div>
            <div class='field'><span class='label'>Department:</span> <span class='value'>{department ?? "General"}</span></div>
            <div class='field'><span class='label'>Requested Access Role:</span> <span class='badge'>{requestedRole}</span></div>
        </div>

        <p>You can review, approve, or adjust this user's role by logging in to the administration portal:</p>

        <div style='text-align: center; margin: 24px 0;'>
            <a href='{approveUrl}' class='btn' target='_blank'>Review & Approve Role</a>
        </div>

        <p style='font-size: 13px; color: #64748b;'>
            Alternatively, navigate to: <br/>
            <code>{approveUrl}</code>
        </p>

        <div class='footer'>
            &copy; 2026 Pranav Enterprises. Automated security dispatch.
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(adminEmail, subject, body);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpSettings = _configuration.GetSection("Smtp");
        var host = smtpSettings["Host"];
        var port = int.TryParse(smtpSettings["Port"], out var p) ? p : 587;
        var username = smtpSettings["Username"];
        var password = smtpSettings["Password"];
        var fromEmail = smtpSettings["FromEmail"] ?? "noreply@pranav.com";
        var fromName = smtpSettings["FromName"] ?? "Pranav Enterprises Portal";
        var enableSsl = bool.TryParse(smtpSettings["EnableSsl"], out var ssl) && ssl;

        // If SMTP server is configured, attempt sending real email via SMTP
        if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(username))
        {
            try
            {
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                using var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);
                await client.SendMailAsync(mail);
                _logger.LogInformation("Registration approval email dispatched successfully to {Recipient} via SMTP.", toEmail);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send email via SMTP to {Recipient}. Falling back to system notification log.", toEmail);
            }
        }

        // Development/Console log fallback
        _logger.LogInformation("\n============================================================\n" +
                               "EMAIL DISPATCHED TO: {Recipient}\n" +
                               "SUBJECT: {Subject}\n" +
                               "BODY PREVIEW:\n{Body}\n" +
                               "============================================================",
                               toEmail, subject, htmlBody);

        await Task.CompletedTask;
    }
}
