namespace Shared.Contracts.Events;

/// <summary>
/// Базовый класс для событий интеграции между микросервисами
/// </summary>
public abstract class IntegrationEvent
{
    public Guid EventId { get; protected set; }
    public DateTime OccurredOn { get; protected set; }
    public string AggregateId { get; protected set; } = string.Empty;

    protected IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    protected IntegrationEvent(Guid aggregateId)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        AggregateId = aggregateId.ToString();
    }
}
