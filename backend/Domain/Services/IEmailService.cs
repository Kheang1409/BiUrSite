namespace Backend.Domain.Services;
public interface IEmailService
{
    Task SendOtpEmail(string recipientEmail, string? otp);
    Task SendConfirmationEmail(string recipientEmail, string confirmationLink);
}
