using System;
using System.Collections.Generic;

namespace UserService.Infrastructure.DataAssets.Users
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public bool IsAdmin { get; set; }

        public List<User> Users { get; set; }
    }
}
