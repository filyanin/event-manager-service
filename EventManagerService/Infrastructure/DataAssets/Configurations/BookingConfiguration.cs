using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventManagerService.Domain.Enum;

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

            builder.HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId);
        }
    }
}
