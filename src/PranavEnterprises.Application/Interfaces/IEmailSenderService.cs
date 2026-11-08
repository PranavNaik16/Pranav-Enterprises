namespace PranavEnterprises.Application.Interfaces;

public interface IEmailSenderService
{
    Task SendRegistrationNotificationAsync(
        string adminEmail,
        string newUserName,
        string newUserEmail,
        string requestedRole,
        string? jobTitle,
        string? department,
        string approveUrl);

    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
