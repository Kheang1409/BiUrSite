namespace Backend.Infrastructure.Configurations;
public class EmailSettings
{
    public required string SmtpServer { get; set; }
    public int Port { get; set; }
    public required string SenderEmail { get; set; }
    public required string SenderPassword { get; set; }

}