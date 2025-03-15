namespace Backend.Services{
    public interface INotificationService
    {
        Task SendOtpEmail(string recipientEmail, string? otp);
        Task SendConfirmationEmail(string recipientEmail, string confirmationLink);
    }
}