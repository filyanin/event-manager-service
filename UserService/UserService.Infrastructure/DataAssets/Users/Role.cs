using System;

namespace UserService.Infrastructure.DataAssets.Users
{
    // Роль пользователя без коллекции пользователей (уменьшенная связность)
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public bool IsAdmin { get; set; }
    }
}
