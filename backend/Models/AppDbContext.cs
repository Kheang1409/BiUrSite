using Backend.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var admin = new User
            {
                userId = 1,
                username = "admin",
                email = "example@gmail.com",
                role = Role.Admin,
                status = Status.Verified,
                password = "$2a$11$FCyLQBXkPuw44t82Fi8Qf.V8pecKGOPcPh59fGkrbfXEjbsMEc6FK", //// Prehashed password. Don't use User.HashPassword("123456"), <- dynamic value 
                createdDate = new DateTime(2025, 3, 4)
            };

            modelBuilder.Entity<User>()
                .HasData(admin);

            modelBuilder.Entity<User>()
                .Property(user => user.role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(user => user.status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .HasCheckConstraint("CK_User_Status", "status IN ('Unverified', 'Verified', 'Banned')");

            modelBuilder.Entity<User>()
                .HasCheckConstraint("CK_User_Role", "role IN ('User', 'Admin')");

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.user)
                .WithMany(u => u.comments)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.post)
                .WithMany(p => p.comments)
                .OnDelete(DeleteBehavior.Restrict);
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
                    ((Comment)entity.Entity).createdDate = DateTime.UtcNow;
                }
                else if (entity.State == EntityState.Modified)
                {
                    ((User)entity.Entity).modifiedDate = DateTime.UtcNow;
                    ((Post)entity.Entity).modifiedDate = DateTime.UtcNow;
                    ((Comment)entity.Entity).modifiedDate = DateTime.UtcNow;
                }
            }
            return base.SaveChanges();
        }
    }
}
