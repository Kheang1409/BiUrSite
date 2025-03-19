using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Backend.Data;

namespace Backend.Services
{
    public class DatabaseService : IDatabaseService
    {
        public void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("SQLConnection")
                                   ?? configuration.GetConnectionString("SQLConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                }));
        }

        public void ApplyDatabaseMigrations(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (!dbContext.Database.CanConnect())
                {
                    try
                    {
                        dbContext.Database.Migrate();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("already exists"))
                        {
                            Console.WriteLine("Database already exists.");
                        }
                        else
                        {
                            Console.WriteLine($"{ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
