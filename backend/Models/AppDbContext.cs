using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }

        [Obsolete]
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .Property(user=> user.role)
                .HasConversion<string>();
             modelBuilder.Entity<User>()
                .Property(user=> user.status)
                .HasConversion<string>();
            modelBuilder.Entity<User>()
                .HasCheckConstraint("CK_User_Status", 
                    "status IN ('Unverified', 'Verified', 'Banned')");
            modelBuilder.Entity<User>()
                .HasCheckConstraint("CK_User_Role", 
                    "role IN ('User', 'Admin')");
        }

        public override int SaveChanges()
        {
            var entities = ChangeTracker.Entries()
                .Where(e => e.Entity is User && 
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((User)entity.Entity).createdDate = DateTime.UtcNow;
                    ((Post)entity.Entity).createdDate = DateTime.UtcNow;
                }
                else if (entity.State == EntityState.Modified)
                {
                    ((User)entity.Entity).modifiedDate = DateTime.UtcNow;
                    ((Post)entity.Entity).modifiedDate = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }
    }
}
