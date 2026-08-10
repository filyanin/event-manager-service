using System;

namespace UserService.Infrastructure.DataAssets.Users
{
    // Модель пользователя без прямых навигационных связей
    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        public string PasswordHash { get; set; }

        // Ссылка на роль в виде Guid (внешняя связь, уменьшенная связность)
        public Guid RoleId { get; set; }
    }
}
