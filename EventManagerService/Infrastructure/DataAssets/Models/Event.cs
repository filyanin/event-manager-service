using System.Collections.Generic;
using System.Linq;

namespace EventManagerService.Infrastructure.DataAssets.Models
{
    public class Event
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public ICollection<Models.Booking> Bookings { get; set; } = new List<Models.Booking>();

        public Event() { }

        public Domain.Models.DomainEvent.DomainEvent ConvertTo()
        {
            var domainEvent = new Domain.Models.DomainEvent.DomainEvent(Id, Title, StartAt, EndAt, TotalSeats, AvailableSeats, Description);

            if (Bookings != null && Bookings.Any())
            {
                var domainBookings = Bookings.Select(b => b.ConvertTo()).ToArray();

                for (int i = 0; i < domainBookings.Length; i++)
                {
                    var booking = domainBookings[i];
                    var eventProp = typeof(EventManagerService.Domain.Models.DomainBooking.DomainBooking).GetProperty("Event");
                    if (eventProp != null && eventProp.CanWrite)
                    {
                        eventProp.SetValue(booking, domainEvent);
                    }
                }
            }

            return domainEvent;
        }
    }

}
