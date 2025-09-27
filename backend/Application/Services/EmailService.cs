using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit.Text;
using Backend.Application.Messaging.Emails;
using Microsoft.Extensions.Configuration;
using Backend.SharedKernel.Exceptions;

namespace Backend.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _server;
    private readonly string _port;
    private readonly string _senderEmail;
    private readonly string _senderPassword;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _server = Environment.GetEnvironmentVariable("SMTP_SERVER")
                        ?? _configuration["SMTP:Server"]
                        ?? throw new InvalidOperationException("SMTP SmtpServer is not configured.");
        _port = Environment.GetEnvironmentVariable("SMTP_PORT")
                        ?? _configuration["SMTP:Port"]
                        ?? throw new InvalidOperationException("SMTP Port is not configured.");
        _senderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL")
                        ?? _configuration["SMTP:SenderEmail"]
                        ?? throw new InvalidOperationException("SMTP SenderEmail is not configured.");

        _senderPassword = Environment.GetEnvironmentVariable("SMTP_SENDER_PASSWORD")
                        ?? _configuration["SMTP:SenderPassword"]
                        ?? throw new InvalidOperationException("SMTP SenderPassword is not configured.");
    }

    public async Task Send<T>(T Email) where T : EmailBase
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("BiUrSite", _server));
            message.To.Add(new MailboxAddress(Email.Recipient, Email.Recipient));
            message.Subject = Email.Subject;
            message.Body = new TextPart(TextFormat.Html) { Text = Email.Message() };
            await SendEmailAsync(message);
        }
        catch (Exception ex)
        {
            throw new EmailSendException("Failed to send email", ex);
        }
    }

    private async Task SendEmailAsync(MimeMessage message)
    {
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_server, int.Parse(_port), SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_senderEmail, _senderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (SmtpCommandException ex)
        {
            throw new EmailSendException($"SMTP command failed: {ex.Message}", ex);
        }
        catch (SmtpProtocolException ex)
        {
            throw new EmailSendException($"SMTP protocol error: {ex.Message}", ex);
        }
    }
}