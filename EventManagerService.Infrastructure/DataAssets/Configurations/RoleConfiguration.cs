using EventManagerService.Infrastructure.DataAssets.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagerService.Infrastructure.DataAssets.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<EventManagerService.Infrastructure.DataAssets.Users.Role>
    {
        public void Configure(EntityTypeBuilder<EventManagerService.Infrastructure.DataAssets.Users.Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedNever();

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.IsAdmin)
                .IsRequired();

            builder.HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey("RoleId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
