using Microsoft.EntityFrameworkCore;
using EventService.Infrastructure.DataAssets.Models;

namespace EventService.Infrastructure.DataAssets
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Event> Events => Set<Event>();

        public DbSet<ProcessedBookingConfirmation> ProcessedBookingConfirmations => Set<ProcessedBookingConfirmation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
