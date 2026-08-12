using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventService.Infrastructure.DataAssets.Models;

namespace EventService.Infrastructure.DataAssets.Configurations
{
    public class ProcessedBookingConfirmationConfiguration : IEntityTypeConfiguration<ProcessedBookingConfirmation>
    {
        public void Configure(EntityTypeBuilder<ProcessedBookingConfirmation> builder)
        {
            builder.ToTable("ProcessedBookingConfirmations");

            builder.HasKey(p => p.BookingId);
            builder.Property(p => p.BookingId).ValueGeneratedNever();
            builder.Property(p => p.ProcessedAt).IsRequired();
        }
    }
}
