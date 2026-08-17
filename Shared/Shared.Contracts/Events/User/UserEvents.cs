namespace Shared.Contracts.Events.User;

public class UserCreatedEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public UserCreatedEvent() { }

    public UserCreatedEvent(Guid userId, string email, string userName) : base(userId)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
    }
}

public class UserUpdatedEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public UserUpdatedEvent() { }

    public UserUpdatedEvent(Guid userId, string email, string userName) : base(userId)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
    }
}

public class UserDeletedEvent : IntegrationEvent
{
    public Guid UserId { get; set; }

    public UserDeletedEvent() { }

    public UserDeletedEvent(Guid userId) : base(userId)
    {
        UserId = userId;
    }
}
