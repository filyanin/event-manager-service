using EventManagerService.Domain.Interfaces.BookingService;

namespace EventManagerService.Infrastructure
{
    public class BookingBackgroundService : BackgroundService
    {
        private IBookingService _bookingService;
        public BookingBackgroundService(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var pendingBookings = await _bookingService.GetBookingByStateAsync(Domain.Enum.BookingStatus.Pending);

                if (pendingBookings.Count > 0)
                {
                    var tasks = new List<Task>();

                    foreach (var booking in pendingBookings)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            await Task.Delay(10000, stoppingToken);

                            await _bookingService.ConfirmBookingAsync(booking.Id);
                        }));
                    }
                }

                await Task.Delay(60000, stoppingToken);

            }
        }
    }
}
