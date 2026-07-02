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

        public Models.Booking[] Bookings { get; set; }

        public Event() { }

        public Domain.Models.Event.Event ConvertTo()
        {
            var domainEvent = new Domain.Models.Event.Event(Id, Title, StartAt, EndAt, TotalSeats, AvailableSeats, Description);

            if (Bookings != null && Bookings.Length > 0)
            {
                var domainBookings = Bookings.Select(b => b.ConvertTo()).ToArray();

                for (int i = 0; i < domainBookings.Length; i++)
                {
                    var booking = domainBookings[i];
                    var eventProp = typeof(EventManagerService.Domain.Models.Booking.Booking).GetProperty("Event");
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
