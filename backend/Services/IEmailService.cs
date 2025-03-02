namespace Backend.Services
{
    public interface IEmailService
    {
        void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration);
    }
}
