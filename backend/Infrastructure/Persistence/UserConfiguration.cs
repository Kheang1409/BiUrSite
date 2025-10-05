using Backend.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).HasConversion(
            userId => userId.Value,
            value => new UserId(value)
        );

        builder.OwnsOne(u => u.Otp, otpBuilder =>
        {
            otpBuilder.Property(opt => opt.Value).HasMaxLength(6);
        });

        builder.OwnsOne(u => u.Token);

        builder.Property(c => c.Username).HasMaxLength(25);
        builder.Property(c => c.Email).HasMaxLength(255);
        builder.HasIndex(c => c.Email).IsUnique();
    }
}