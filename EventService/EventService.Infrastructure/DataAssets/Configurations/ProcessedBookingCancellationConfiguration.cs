using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventService.Infrastructure.DataAssets.Models;

namespace EventService.Infrastructure.DataAssets.Configurations
{
    public class ProcessedBookingCancellationConfiguration : IEntityTypeConfiguration<ProcessedBookingCancellation>
    {
        public void Configure(EntityTypeBuilder<ProcessedBookingCancellation> builder)
        {
            builder.ToTable("ProcessedBookingCancellations");

            builder.HasKey(p => p.BookingId);
            builder.Property(p => p.BookingId).ValueGeneratedNever();
            builder.Property(p => p.ProcessedAt).IsRequired();
        }
    }
}
