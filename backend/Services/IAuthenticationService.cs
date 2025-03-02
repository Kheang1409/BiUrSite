namespace Backend.Services
{
    public interface IAuthenticationService
    {
        void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration);
    }
}
