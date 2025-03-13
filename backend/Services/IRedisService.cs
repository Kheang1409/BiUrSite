namespace Backend.Services
{
    public interface IRedisService
    {
        void ConfigureRedis(IServiceCollection services, IConfiguration configuration);
    }
}
