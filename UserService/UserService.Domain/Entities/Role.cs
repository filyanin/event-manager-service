namespace UserService.Domain.Entities;

/// <summary>
/// Доменная модель роли пользователя
/// </summary>
public class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
}
