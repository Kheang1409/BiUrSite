using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.Data.SqlClient;

namespace Backend.Services
{
    public class DatabaseService : IDatabaseService
    {
        public void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("SqlConnection")
                                   ?? configuration.GetConnectionString("DefaultConnection");

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
