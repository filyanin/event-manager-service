using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventService.Infrastructure.DataAssets.Models;

namespace EventService.Infrastructure.DataAssets.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("Events");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Title).IsRequired().HasMaxLength(1000);
            builder.Property(e => e.Description).HasMaxLength(5000);
            builder.Property(e => e.StartAt).IsRequired();
            builder.Property(e => e.EndAt).IsRequired();
            builder.Property(e => e.TotalSeats).IsRequired();
            builder.Property(e => e.AvailableSeats).IsRequired();
            builder.Property(e => e.CreatedByUserId).IsRequired();
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedAt);
            builder.Property(e => e.Timestamp).IsRowVersion();

            // Индекс для быстрого поиска событий по CreatedByUserId
            builder.HasIndex(e => e.CreatedByUserId);

            // Индекс для быстрого поиска событий по дате
            builder.HasIndex(e => e.StartAt);
        }
    }
}
