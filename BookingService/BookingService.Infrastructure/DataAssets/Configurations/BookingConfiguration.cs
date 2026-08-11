using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingService.Infrastructure.DataAssets.Models;

namespace BookingService.Infrastructure.DataAssets.Configurations;

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

        // Ссылки на внешние сервисы как GUID (без FK)
        builder.Property(b => b.EventGuid).IsRequired();
        builder.Property(b => b.UserGuid).IsRequired();
        builder.Property(b => b.SeatsBooked).IsRequired();

        // Индексы для быстрого поиска
        builder.HasIndex(b => b.EventGuid);
        builder.HasIndex(b => b.UserGuid);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.CreatedAt);
    }
}
