using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagerService.Infrastructure.DataAssets.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Models.Booking>
    {
        public void Configure(EntityTypeBuilder<Models.Booking> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .ValueGeneratedNever();

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            builder.Property(b => b.ProcessedAt)
                .IsRequired(false);

            builder.HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId);

            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey("UserId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
