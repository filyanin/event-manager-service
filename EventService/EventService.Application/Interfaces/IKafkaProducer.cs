namespace EventService.Application.Interfaces;

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, string key, T message);
}
