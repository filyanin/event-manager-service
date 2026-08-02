using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Infrastructure.DataAssets.Users
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public bool IsAdmin { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
