using Backend.Models;

namespace Backend.Services
{
    public class EmailService : IEmailService
    {
        public void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(
                configuration.GetSection(nameof(EmailSettings)));
        }
    }
}
