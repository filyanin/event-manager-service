using EventManagerService.Infrastructure.DataAssets.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Infrastructure.DataAssets.Users
{
    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        public string PasswordHash { get; set; }

        public Role Role { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}
