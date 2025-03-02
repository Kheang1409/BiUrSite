namespace Backend.Services
{
    public interface IDatabaseService
    {
        void ConfigureDatabase(IServiceCollection services, IConfiguration configuration);
        void ApplyDatabaseMigrations(IApplicationBuilder app);
    }
}
