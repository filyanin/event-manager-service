using Microsoft.EntityFrameworkCore;
using System;
using UserService.Infrastructure.DataAssets.Users;

namespace UserService.Infrastructure.DataAssets
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "user", IsAdmin = false },
                new Role { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "admin", IsAdmin = true }
            );
        }
    }
}
