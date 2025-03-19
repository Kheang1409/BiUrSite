using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit.Text;
using Microsoft.Extensions.Options;
using Backend.Models;

namespace Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
        }

        public async Task SendOtpEmail(string recipientEmail, string? otp)
        {
            var emailBody = $"Your OTP to reset your password is: {otp}. It will expire in 3 minutes.";
            var message = CreateEmailMessage(recipientEmail, "Password Reset OTP", emailBody);
            await SendEmailAsync(message);
        }

        public async Task SendConfirmationEmail(string recipientEmail, string confirmationLink)
        {
            var emailBody = $@"
                <h1>Verify Your Account</h1>
                <p>Please click the link below to verify your account:</p>
                <a href='{confirmationLink}'>{confirmationLink}</a>
                <p>If you did not request this, please ignore this email.</p>";

            var message = CreateEmailMessage(recipientEmail, "Confirm Your Email Address", emailBody);
            await SendEmailAsync(message);
        }

        private MimeMessage CreateEmailMessage(string recipientEmail, string subject, string bodyText)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("BiUrSiteApp", _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(recipientEmail, recipientEmail));
            message.Subject = subject;

            message.Body = new TextPart(TextFormat.Html) { Text = bodyText };

            return message;
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }
    }
}
