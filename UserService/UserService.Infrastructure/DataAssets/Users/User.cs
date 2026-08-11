using System;

namespace UserService.Infrastructure.DataAssets.Users
{
    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        public string PasswordHash { get; set; }

        public Guid RoleId { get; set; }

        public Role Role { get; set; }
    }
}
