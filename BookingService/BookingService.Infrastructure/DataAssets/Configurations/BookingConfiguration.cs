using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingService.Infrastructure.DataAssets.Models;

namespace BookingService.Infrastructure.DataAssets.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).ValueGeneratedNever();

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.ProcessedAt).IsRequired(false);

            // Уменьшенная связность: не настраиваем навигационные связи к Event или User
            builder.Property(b => b.EventId).IsRequired();
            builder.Property(b => b.UserId).IsRequired();
        }
    }
}
