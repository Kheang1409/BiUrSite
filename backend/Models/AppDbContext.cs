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
                profile = "assets/img/profile-default.svg",
                role = Role.Admin,
                status = Status.Verified,
                password = "$2a$11$FCyLQBXkPuw44t82Fi8Qf.V8pecKGOPcPh59fGkrbfXEjbsMEc6FK" // Prehashed password. Don't use User.HashPassword("123456"), <- dynamic value 
            };

            modelBuilder.Entity<User>()
                .HasData(admin);

            modelBuilder.Entity<User>()
                .Property(user => user.role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(user => user.status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()?
                .ToTable("Users", userTable => userTable.HasCheckConstraint("CK_User_Status", "status IN ('Unverified', 'Verified', 'Banned', 'Deleted')"));

            modelBuilder.Entity<User>()?
                .ToTable("Users", userTable => userTable.HasCheckConstraint("CK_User_Role", "role IN ('User', 'Admin')"));

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.commenter)
                .WithMany(u => u.comments)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Post>()
                .HasOne(p => p.author)
                .WithMany(u => u.posts)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.post)
                .WithMany(p => p.comments)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .Property(u => u.createdDate)
                .HasDefaultValueSql("getutcdate()")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Post>()
                .Property(p => p.createdDate)
                .HasDefaultValueSql("getutcdate()")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Comment>()
                .Property(c => c.createdDate)
                .HasDefaultValueSql("getutcdate()")
                .ValueGeneratedOnAdd();
        }
    }
}
