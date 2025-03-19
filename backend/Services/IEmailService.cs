namespace Backend.Services
{
    public interface IEmailService
    {
        void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration);
        Task SendOtpEmail(string recipientEmail, string? otp);
        Task SendConfirmationEmail(string recipientEmail, string confirmationLink);
    }
}
