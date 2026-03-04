using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit.Text;
using Backend.Application.Messaging.Emails;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Backend.SharedKernel.Exceptions;

namespace Backend.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _server;
    private readonly string _port;
    private readonly string _senderEmail;
    private readonly string _senderPassword;
    private readonly ILogger<EmailService> _logger;
    private static readonly ConcurrentDictionary<string, DateTime> _lastSent = new();
    private static readonly TimeSpan _minIntervalPerRecipient = TimeSpan.FromSeconds(30);

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        if (Email == null) throw new ArgumentNullException(nameof(Email));
        if (string.IsNullOrWhiteSpace(Email.Recipient)) throw new ArgumentException("Recipient email is required.", nameof(Email));
        try
        {
            if (_lastSent.TryGetValue(Email.Recipient, out var last) )
            {
                var elapsed = DateTime.UtcNow - last;
                if (elapsed < _minIntervalPerRecipient)
                {
                    var wait = _minIntervalPerRecipient - elapsed;
                    _logger?.LogInformation("Throttling email to {recipient} for {delay} to avoid recipient receiving-rate limits.", Email.Recipient, wait);
                    await Task.Delay(wait);
                }
            }

        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("BiUrSite", _senderEmail));
            message.Sender = new MailboxAddress("BiUrSite", _senderEmail);
            message.To.Add(new MailboxAddress(Email.Recipient, Email.Recipient));
            message.Subject = Email.Subject;
            message.Body = new TextPart(TextFormat.Html) { Text = Email.Message() };
            await SendEmailAsync(message);
            _lastSent.AddOrUpdate(Email.Recipient, DateTime.UtcNow, (_, __) => DateTime.UtcNow);
        }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send email to {recipient}", Email.Recipient);
            throw new EmailSendException("Failed to send email", ex);
        }
    }

    private async Task SendEmailAsync(MimeMessage message)
    {
        const int maxAttempts = 8;
        var delay = TimeSpan.FromSeconds(2);
        var rng = new Random();

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_server, int.Parse(_port), SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_senderEmail, _senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger?.LogInformation("Email sent to {recipients} on attempt {attempt}", string.Join(',', message.To), attempt);
                return;
            }
            catch (SmtpCommandException ex)
            {
                var fullText = ex.ToString();
                var isRateLimit = Regex.IsMatch(fullText ?? string.Empty, @"\b4\.2\.1\b");
                if (isRateLimit)
                {
                    _logger?.LogWarning(ex, "Rate-limited by recipient server (4.2.1) on attempt {attempt}. Will retry with extended backoff.", attempt);
                    if (attempt == maxAttempts)
                        throw new EmailSendException($"SMTP command failed after {maxAttempts} attempts (rate-limited): {ex.Message}", ex);

                    var jitter = TimeSpan.FromMilliseconds(rng.Next(1000, 5000));
                    var extended = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, attempt) * 5, 900));
                    await Task.Delay(extended + jitter);
                    continue;
                }

                _logger?.LogWarning(ex, "SmtpCommandException (attempt {attempt}): {message}", attempt, ex.Message);
                if (attempt == maxAttempts)
                    throw new EmailSendException($"SMTP command failed after {maxAttempts} attempts: {ex.Message}", ex);

                var jitterNormal = TimeSpan.FromMilliseconds(rng.Next(100, 1000));
                await Task.Delay(delay + jitterNormal);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
                continue;
            }
            catch (SmtpProtocolException ex)
            {
                _logger?.LogWarning(ex, "SmtpProtocolException (attempt {attempt}): {message}", attempt, ex.Message);
                if (attempt == maxAttempts)
                    throw new EmailSendException($"SMTP protocol error after {maxAttempts} attempts: {ex.Message}", ex);

                var jitter = TimeSpan.FromMilliseconds(rng.Next(100, 1000));
                await Task.Delay(delay + jitter);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
                continue;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error while sending email to {recipients}", string.Join(',', message.To));
                throw new EmailSendException("Unexpected error while sending email", ex);
            }
        }
    }
}