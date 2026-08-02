using EventManagerService.Infrastructure.DataAssets.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagerService.Infrastructure.DataAssets.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .ValueGeneratedNever();

            builder.Property(u => u.Login)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey("RoleId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey("UserId");
        }
    }
}
