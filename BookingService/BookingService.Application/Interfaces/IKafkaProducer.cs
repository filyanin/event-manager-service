using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Application.Interfaces
{
    public interface IKafkaProducer
    {
        Task PublishAsync<T>(string topic, string key, T message);
    }
}
