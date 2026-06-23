using Microsoft.EntityFrameworkCore;

namespace EventManagerService.Infrastructure.DataAssets
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Models.Event> Events => Set<Models.Event>();
        public DbSet<Models.Booking> Bookings => Set<Models.Booking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
