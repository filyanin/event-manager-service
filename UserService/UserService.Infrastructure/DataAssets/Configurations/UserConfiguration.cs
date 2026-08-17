using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Infrastructure.DataAssets.Users;

namespace UserService.Infrastructure.DataAssets.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Login).IsRequired().HasMaxLength(200);
            builder.HasIndex(u => u.Login).IsUnique();

            builder.Property(u => u.PasswordHash).IsRequired();

            builder.Property(u => u.RoleId).IsRequired();

            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
