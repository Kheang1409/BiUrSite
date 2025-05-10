using Backend.Domain.Comments.Entities;
using Backend.Domain.Notifications.Entities;
using Backend.Domain.Posts.Entities;
using Backend.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(user => user.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(user => user.Status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()?
                .ToTable("Users", userTable => userTable.HasCheckConstraint("CK_User_Status", "status IN ('Active', 'Deactivated', 'Unverified', 'Banned', 'Deleted')"));

            modelBuilder.Entity<User>()?
                .ToTable("Users", userTable => userTable.HasCheckConstraint("CK_User_Role", "role IN ('User', 'Admin')"));

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Commenter)
                .WithMany(u => u.Comments)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Post>()
                .Property(p => p.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Comment>()
                .Property(c => c.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Notification>()
                .Property(n => n.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
                .ValueGeneratedOnAdd();
        }
    }
}
