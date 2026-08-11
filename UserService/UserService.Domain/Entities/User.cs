namespace UserService.Domain.Entities;

/// <summary>
/// Доменная модель пользователя
/// </summary>
public class User
{
    public Guid Id { get; set; }

    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Guid RoleId { get; set; }

    public Role? Role { get; set; }
}
